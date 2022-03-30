using HeaderPropagation.B3Headers.Constants;
using HeaderPropagation.B3Headers.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HeaderPropagation.B3Headers
{
	public class B3HeadersPropagationDelegatingHandler : DelegatingHandler
	{
		private readonly IHttpContextAccessor httpContextAccessor;
		private readonly ILogger<B3HeadersPropagationDelegatingHandler> logger;

		public B3HeadersPropagationDelegatingHandler(IHttpContextAccessor httpContextAccessor, ILogger<B3HeadersPropagationDelegatingHandler> logger)
		{
			this.httpContextAccessor = httpContextAccessor;
			this.logger = logger;
		}

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var b3Headers = httpContextAccessor.HttpContext?.RequestServices.GetService<IB3HeadersHolder>();

            if (b3Headers != null)
            {
                AddHeaderIfNotNull(request, B3HeaderConstants.REQUEST_ID, b3Headers.RequestId);
                AddHeaderIfNotNull(request, B3HeaderConstants.B3_TRACE_ID, b3Headers.B3TraceId);
                AddHeaderIfNotNull(request, B3HeaderConstants.B3_SPAN_ID, b3Headers.B3SpanId);
                AddHeaderIfNotNull(request, B3HeaderConstants.B3_PARENT_SPAN_ID, b3Headers.B3ParentSpanId);
                AddHeaderIfNotNull(request, B3HeaderConstants.B3_SAMPLED, b3Headers.B3Sampled);
                AddHeaderIfNotNull(request, B3HeaderConstants.B3_FLAGS, b3Headers.B3Flags);
                AddHeaderIfNotNull(request, B3HeaderConstants.OT_SPAN_CONTEXT, b3Headers.OtSpanContext);
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