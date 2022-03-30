using EventBusMessages.Common;
using HealthChecks.UI.Client;
using MassTransit;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Exporter;
using OpenTelemetry.Trace;
using Orders.API.EventBusConsumer;
using Orders.Application;
using Orders.Application.Features.Orders.Commands.CheckoutOrder;
using Orders.Infrastructure;
using TelemetryKitchenSink;

namespace Orders.API
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

        // This method gets called by the runtime. Use this method to add services to the container.
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

            services.AddApplicationServices(Configuration);
            services.AddInfrastructureServices(Configuration);
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // MassTransit-RabbitMQ Configuration
            services.AddMassTransit(config => {

                config.AddConsumer<CartCheckoutConsumer>();

                config.UsingRabbitMq((ctx, cfg) => {
                    cfg.Host(Configuration["EventBusSettings:HostAddress"]);

                    cfg.ReceiveEndpoint(EventBusConstants.CartCheckoutQueue, c => {
                        c.ConfigureConsumer<CartCheckoutConsumer>(ctx);
                    });
                });
            });
            services.AddMassTransitHostedService();

            // General Configuration
            services.AddScoped<CartCheckoutConsumer>();
            services.AddAutoMapper(typeof(Startup));

            services.AddControllers();

            // API versioning
            services.AddApiVersioning(setup =>
            {
                setup.DefaultApiVersion = new ApiVersion(1, 0);
                setup.AssumeDefaultVersionWhenUnspecified = true;
                setup.ReportApiVersions = true;
            });

            var logger = services.BuildServiceProvider().GetRequiredService<ILogger<TelemetryBaggageLogger>>();
            var context = services.BuildServiceProvider().GetRequiredService<IHttpContextAccessor>();

            // OpenTelemetry
            services.AddOpenTelemetryTracing(builder =>
            {
                string[] listOfSources = new string[] {
                    CurrentEnvironment.ApplicationName,
                    nameof(CheckoutOrderCommandHandler),
                    nameof(CartCheckoutConsumer)
                };
                builder
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.Enrich = TelemetryAspNetCoreEnricher.EnrichHttpRequests;
                        options.EnableGrpcAspNetCoreSupport = true;
                        options.Filter = (httpContext) =>
                        {
                            // We dont care about the health endpoint
                            return !httpContext.Request.Path.Equals("/health");
                        };
                    })
                    .AddHttpClientInstrumentation()
                    .AddGrpcClientInstrumentation()
                    .AddSource(listOfSources)
                    .AddProcessor(new TelemetryBaggageLogger(logger, context))
                    .SetResourceBuilder(ResourceBuilderGenerator.GetResourceBuilder(CurrentEnvironment));

                if (CurrentEnvironment.IsDevelopment())
                {
                    builder.AddConsoleExporter(opt => {
                        opt.Targets = ConsoleExporterOutputTargets.Debug;
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

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Orders.API", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Orders.API v1"));
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
            });
        }
    }
}

