using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Exporter;
using OpenTelemetry.Trace;
using Products.API.Controllers;
using Products.API.Data;
using Products.API.Data.Interfaces;
using Products.API.Repositories;
using Products.API.Repositories.Interfaces;
using TelemetryKitchenSink;

namespace Products.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            CurrentEnvironment = env;
        }

        public IConfiguration Configuration { get; }
        private IWebHostEnvironment CurrentEnvironment { get; set; }

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

            services.AddScoped<IProductContext, ProductContext>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

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
                    nameof(ProductsController)
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
                    builder.AddConsoleExporter(opt => {
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

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Products.API", Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Products.API v1"));
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