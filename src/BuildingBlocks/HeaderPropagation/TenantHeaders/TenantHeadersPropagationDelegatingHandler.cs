using HeaderPropagation.TenantHeaders.Constants;
using HeaderPropagation.TenantHeaders.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HeaderPropagation.TenantHeaders
{
	public class TenantHeadersPropagationDelegatingHandler : DelegatingHandler
	{
		private readonly IHttpContextAccessor httpContextAccessor;
		private readonly ILogger<TenantHeadersPropagationDelegatingHandler> logger;

		public TenantHeadersPropagationDelegatingHandler(IHttpContextAccessor httpContextAccessor, ILogger<TenantHeadersPropagationDelegatingHandler> logger)
		{
			this.httpContextAccessor = httpContextAccessor;
			this.logger = logger;
		}

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var tenantHeaders = httpContextAccessor.HttpContext?.RequestServices.GetService<ITenantHeadersHolder>();

            if (tenantHeaders != null)
            {
                AddHeaderIfNotNull(request, TenantHeaderConstants.TENANT_ID, tenantHeaders.TenantId);
                AddHeaderIfNotNull(request, TenantHeaderConstants.USER_ID, tenantHeaders.UserId);
            }

            return base.SendAsync(request, cancellationToken);
        }

        private void AddHeaderIfNotNull(HttpRequestMessage request, string headerName, string headerValue)
        {
            if (!string.IsNullOrWhiteSpace(headerValue))
            {
                request.Headers.TryAddWithoutValidation(headerName, headerValue);
            }
            else
            {
                logger.LogTrace("Not adding header {headerName} to the client. It is null or empty.", headerName);
            }
        }
    }
}