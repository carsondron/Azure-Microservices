using BuildInfoGenerator;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace LoggingHandler
{
    public static class SeriLogger
    {
        public static void SetupLoggerConfiguration(string appName, BuildInfo buildInfo)
        {
            Log.Logger = new LoggerConfiguration()
                .ConfigureBaseLogging(appName, buildInfo)
                .CreateLogger();
        }

        public static Action<String, HostBuilderContext, IServiceProvider, LoggerConfiguration> Configure =>
            (appName, context, services, configuration) =>
            {
                ConfigureBaseLogging(configuration, appName, AppVersionInfo.GetBuildInfo());
                AddApplicationInsightsLogging(configuration, services, context.Configuration);
            };

        internal static LoggerConfiguration ConfigureBaseLogging(this LoggerConfiguration loggerConfiguration, string appName, BuildInfo buildInfo)
        {
            loggerConfiguration
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .WriteTo.Async(a => a.Console(theme: AnsiConsoleTheme.Code))
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .Enrich.WithProperty(nameof(buildInfo.BuildId), buildInfo.BuildId)
                .Enrich.WithProperty(nameof(buildInfo.BuildNumber), buildInfo.BuildNumber)
                .Enrich.WithProperty(nameof(buildInfo.BranchName), buildInfo.BranchName)
                .Enrich.WithProperty(nameof(buildInfo.CommitHash), buildInfo.CommitHash)
                .Enrich.WithProperty("ApplicationName", appName);

            return loggerConfiguration;
        }

        internal static LoggerConfiguration AddApplicationInsightsLogging(this LoggerConfiguration loggerConfiguration, IServiceProvider services, IConfiguration configuration)
        {
            if (!string.IsNullOrWhiteSpace(configuration.GetValue<string>("AppInsights:InstrumentationKey")))
            {
                loggerConfiguration.WriteTo.ApplicationInsights(
                    services.GetRequiredService<TelemetryConfiguration>(),
                    TelemetryConverter.Traces);
            }

            return loggerConfiguration;
        }
    }
}
