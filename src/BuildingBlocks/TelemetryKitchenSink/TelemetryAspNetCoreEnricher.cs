using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using OpenTelemetry;
using TelemetryKitchenSink.Constants;

namespace TelemetryKitchenSink
{
	public static class TelemetryAspNetCoreEnricher
	{
        public static void EnrichHttpRequests(Activity activity, string eventName, object rawObject)
        {
            if (eventName.Equals("OnStartActivity"))
            {
                if (rawObject is HttpRequest request)
                {
                    var context = request.HttpContext;

                    if (context.Request.Headers.TryGetValue(RequestHeaderConstants.TenantId, out var tenantIdHeader))
                    {
                        Baggage.SetBaggage(OpenTelemetryAttributes.OrganizationBaggages.TenantId, tenantIdHeader[0]);
                        activity?.AddTag(OpenTelemetryAttributes.OrganizationBaggages.TenantId, tenantIdHeader[0]);
                    }
                    if (context.Request.Headers.TryGetValue(RequestHeaderConstants.UserId, out var userIdHeader))
                    {
                        Baggage.SetBaggage(OpenTelemetryAttributes.OrganizationBaggages.UserId, userIdHeader[0]);
                        activity?.AddTag(OpenTelemetryAttributes.OrganizationBaggages.UserId, userIdHeader[0]);
                    }
                }
            }
        }
    }
}