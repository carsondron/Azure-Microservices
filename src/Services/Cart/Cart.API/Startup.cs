using Cart.API.Controllers;
using Cart.API.Extensions;
using Cart.API.Repositories;
using Cart.API.Repositories.Interfaces;
using HealthChecks.UI.Client;
using MassTransit;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.OpenApi.Models;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Exporter;
using OpenTelemetry.Trace;
using TelemetryKitchenSink;

namespace Cart.API
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

            // Redis Configuration
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = Configuration.GetValue<string>("CacheSettings:ConnectionString");
            });

            // General Configuration
            services.AddScoped<ICartRepository, CartRepository>();
            services.AddAutoMapper(typeof(Startup));
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // MassTransit-RabbitMQ Configuration
            services.AddMassTransit(config => {
                config.UsingRabbitMq((ctx, cfg) => {
                    cfg.Host(Configuration["EventBusSettings:HostAddress"]);
                });
            });
            services.AddMassTransitHostedService();

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
                    nameof(CartController)
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
                    .Configure((sp, builder) =>
                    {
                        RedisCache cache = (RedisCache)sp.GetRequiredService<IDistributedCache>();
                        builder.AddRedisInstrumentation(cache.GetConnection());
                    })
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
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Cart.API", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cart.API v1"));
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