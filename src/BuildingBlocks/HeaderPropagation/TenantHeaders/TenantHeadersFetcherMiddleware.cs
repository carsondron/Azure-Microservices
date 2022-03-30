using HeaderPropagation.TenantHeaders.Constants;
using HeaderPropagation.TenantHeaders.Interfaces;
using Microsoft.AspNetCore.Http;

namespace HeaderPropagation.TenantHeaders
{
	public class TenantHeadersFetcherMiddleware
	{
		private readonly RequestDelegate _next;

        public TenantHeadersFetcherMiddleware(RequestDelegate next)
        {
            this._next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task InvokeAsync(HttpContext context, ITenantHeadersHolder tenantHeaders)
        {

            tenantHeaders.TenantId = GetHeader(context, TenantHeaderConstants.TENANT_ID);
            tenantHeaders.UserId = GetHeader(context, TenantHeaderConstants.USER_ID);

            await _next(context);
        }

        private string GetHeader(HttpContext context, string headerName)
        {
            if (context.Request.Headers.TryGetValue(headerName, out var values))
            {
                var firstValue = values.FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(firstValue))
                    return firstValue;
            }

            return null;
        }
    }
}