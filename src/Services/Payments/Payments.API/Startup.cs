using EventBusMessages.Common;
using HealthChecks.UI.Client;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Exporter;
using OpenTelemetry.Trace;
using Payments.API.EventBusConsumer;
using TelemetryKitchenSink;

namespace Payments.API
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

            // General Configuration
            services.AddScoped<PaymentExecutionConsumer>();
            services.AddAutoMapper(typeof(Startup));
            services.AddMediatR(typeof(Startup));
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // MassTransit-RabbitMQ Configuration
            services.AddMassTransit(config => {

                config.AddConsumer<PaymentExecutionConsumer>();

                config.UsingRabbitMq((ctx, cfg) => {
                    cfg.Host(Configuration["EventBusSettings:HostAddress"]);

                    cfg.ReceiveEndpoint(EventBusConstants.PaymentProcessorQueue, c => {
                        c.ConfigureConsumer<PaymentExecutionConsumer>(ctx);
                    });
                });
            });
            services.AddMassTransitHostedService();

            services.AddControllers();

            var logger = services.BuildServiceProvider().GetRequiredService<ILogger<TelemetryBaggageLogger>>();
            var context = services.BuildServiceProvider().GetRequiredService<IHttpContextAccessor>();

            // OpenTelemetry
            services.AddOpenTelemetryTracing(builder =>
            {
                string[] listOfSources = new string[] {
                    CurrentEnvironment.ApplicationName,
                    nameof(PaymentExecutionConsumer)
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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

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