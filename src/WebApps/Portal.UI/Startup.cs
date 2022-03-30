using System.Configuration;
using HealthChecks.UI.Client;
using LoggingHandler;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Exporter;
using OpenTelemetry.Trace;
using Portal.UI.Services;
using Portal.UI.Services.Interfaces;
using TelemetryKitchenSink;

namespace Portal.UI
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            CurrentEnvironment = env;
        }

        public IConfiguration Configuration { get; }
        private IWebHostEnvironment CurrentEnvironment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            if (Configuration.GetValue<bool>("ApiSettings:Telemetry:EnableB3Propagation"))
            {
                Sdk.SetDefaultTextMapPropagator(new CompositeTextMapPropagator(new TextMapPropagator[]
                {
                    new TraceContextPropagator(),
                    new B3Propagator(),
                    new BaggagePropagator()
                }));
            }

            services.AddTransient<LoggingDelegatingHandler>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            string productsServiceUri;
            if (!String.IsNullOrEmpty(Configuration["ApiSettings:ProductsServiceAddress"]))
            {
                productsServiceUri = Configuration["ApiSettings:ProductsServiceAddress"];
            }
            else
            {
                productsServiceUri = Configuration["ApiSettings:ApiGatewayAddress"];
            }
            string ordersServiceUri;
            if (!String.IsNullOrEmpty(Configuration["ApiSettings:OrdersServiceAddress"]))
            {
                ordersServiceUri = Configuration["ApiSettings:OrdersServiceAddress"];
            }
            else
            {
                ordersServiceUri = Configuration["ApiSettings:ApiGatewayAddress"];
            }
            string cartServiceUri;
            if (!String.IsNullOrEmpty(Configuration["ApiSettings:CartServiceAddress"]))
            {
                cartServiceUri = Configuration["ApiSettings:CartServiceAddress"];
            }
            else
            {
                cartServiceUri = Configuration["ApiSettings:ApiGatewayAddress"];
            }


            if (!String.IsNullOrEmpty(productsServiceUri))
            {
                services.AddHttpClient<IProductsService, ProductsService>(c => c.BaseAddress = new Uri(productsServiceUri))
                        .AddHttpMessageHandler<LoggingDelegatingHandler>();
            } else
            {
                throw new ConfigurationErrorsException("Products service URI not defined, atleast ApiSettings:ApiGatewayAddress must be defined");
            }

            if (!String.IsNullOrEmpty(ordersServiceUri))
            {
                services.AddHttpClient<IOrdersService, OrdersService>(c => c.BaseAddress = new Uri(ordersServiceUri))
                        .AddHttpMessageHandler<LoggingDelegatingHandler>();
                        
            } else
            {
                throw new ConfigurationErrorsException("Orders service URI not defined, atleast ApiSettings:ApiGatewayAddress must be defined");
            }

            if (!String.IsNullOrEmpty(cartServiceUri))
            {
                services.AddHttpClient<ICartService, CartService>(c => c.BaseAddress = new Uri(cartServiceUri))
                        .AddHttpMessageHandler<LoggingDelegatingHandler>();
            } else
            {
                throw new ConfigurationErrorsException("Cart service URI not defined, atleast ApiSettings:ApiGatewayAddress must be defined");
            }

            // Validating auth
            services.AddAuthentication(options => options.DefaultScheme = TokenAuth.TokenAuthenticationSchemeConstants.AuthScheme)
                .AddScheme<TokenAuth.TokenAuthSchemeOptions, TokenAuth.TokenAuthHandler>(
                    TokenAuth.TokenAuthenticationSchemeConstants.AuthScheme, o => { });

            var logger = services.BuildServiceProvider().GetRequiredService<ILogger<TelemetryBaggageLogger>>();
            var context = services.BuildServiceProvider().GetRequiredService<IHttpContextAccessor>();

            // OpenTelemetry
            services.AddOpenTelemetryTracing((builder) =>
            {
                string[] listOfSources = new string[] {
                    CurrentEnvironment.ApplicationName
                };
                builder
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.Enrich = TelemetryAspNetCoreEnricher.EnrichHttpRequests;
                        options.Filter = (httpContext) =>
                        {
                            // We dont care about the health endpoint
                            return !httpContext.Request.Path.Equals("/health");
                        };
                    })
                    .AddHttpClientInstrumentation()
                    .AddSource(listOfSources)
                    .AddProcessor(new TelemetryBaggageLogger(logger, context))
                    .SetResourceBuilder(ResourceBuilderGenerator.GetResourceBuilder(CurrentEnvironment));

                if (CurrentEnvironment.IsDevelopment())
                {
                    builder.AddConsoleExporter(opt =>
                    {
                        opt.Targets = ConsoleExporterOutputTargets.Console;
                    });
                    if (!String.IsNullOrEmpty(Configuration["ApiSettings:Opentelemetry:OtlpExporterAddress"]))
                    {
                        builder.AddOtlpExporter(opt =>
                        {
                            opt.Endpoint = new Uri(Configuration["ApiSettings:Opentelemetry:OtlpExporterAddress"]);
                        });
                    }
                }
            });

            services.AddHealthChecks();
            services.AddRazorPages()
                .WithRazorPagesRoot("/Views");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapHealthChecks("/health", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
            });
        }
    }
}