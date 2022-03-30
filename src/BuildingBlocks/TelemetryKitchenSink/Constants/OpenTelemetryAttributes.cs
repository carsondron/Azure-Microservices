namespace TelemetryKitchenSink.Constants
{
    public static class OpenTelemetryAttributes
    {
        public static class DefaultAttributes
        {
            public static class Deployment
            {
                public const string Environment = "deployment.environment";
            }
            public static class Host
            {
                public const string Name = "host.name";
            }
        }
        public static class OrganizationBaggages
        {
            public const string TenantId = "tenant.id";
            public const string UserId = "user.id";
        }
    }
}