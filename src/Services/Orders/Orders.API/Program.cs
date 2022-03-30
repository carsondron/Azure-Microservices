using BuildInfoGenerator;
using LoggingHandler;
using Orders.API.Extensions;
using Orders.Infrastructure.Persistence;
using Serilog;

namespace Orders.API
{
    public class Program
    {
        const string APP_NAME = "Orders.API";
        public static int Main(string[] args)
        {
            AppVersionInfo.InitialiseBuildInfoGivenPath(Directory.GetCurrentDirectory());
            SeriLogger.SetupLoggerConfiguration(APP_NAME, AppVersionInfo.GetBuildInfo());

            try
            {
                Log.Information("Starting orders service");
                CreateHostBuilder(args).Build().MigrateDatabase<OrderContext>((context, services) =>
                {
                    var logger = services.GetService<ILogger<OrderContextSeed>>();
                    OrderContextSeed
                        .SeedAsync(context, logger)
                        .Wait();
                }).Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Service terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((hostBuilderContext, services, loggerConfiguration) => SeriLogger.Configure(APP_NAME, hostBuilderContext, services, loggerConfiguration))
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
