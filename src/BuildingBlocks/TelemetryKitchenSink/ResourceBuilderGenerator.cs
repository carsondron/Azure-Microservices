using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using OpenTelemetry.Resources;
using TelemetryKitchenSink.Constants;

namespace TelemetryKitchenSink;

public static class ResourceBuilderGenerator
{
    public static ResourceBuilder GetResourceBuilder(IWebHostEnvironment webHostEnvironment)
    {
        var version = Assembly
            .GetExecutingAssembly()
            .GetCustomAttribute<AssemblyFileVersionAttribute>()!
            .Version;
        return ResourceBuilder
            .CreateEmpty()
            .AddService(webHostEnvironment.ApplicationName, serviceVersion: version)
            .AddAttributes(
                new KeyValuePair<string, object>[]
                {
                    new(OpenTelemetryAttributes.DefaultAttributes.Deployment.Environment, webHostEnvironment.EnvironmentName),
                    new(OpenTelemetryAttributes.DefaultAttributes.Host.Name, Environment.MachineName)
                })
            .AddEnvironmentVariableDetector();
    }
}