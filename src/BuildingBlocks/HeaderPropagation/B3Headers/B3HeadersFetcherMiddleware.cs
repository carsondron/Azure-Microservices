using HeaderPropagation.B3Headers.Constants;
using HeaderPropagation.B3Headers.Interfaces;
using Microsoft.AspNetCore.Http;

namespace HeaderPropagation.B3Headers
{
	public class B3HeadersFetcherMiddleware
	{
		private readonly RequestDelegate _next;

        public B3HeadersFetcherMiddleware(RequestDelegate next)
        {
            this._next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task InvokeAsync(HttpContext context, IB3HeadersHolder b3Headers)
        {

            b3Headers.RequestId = GetHeader(context, B3HeaderConstants.REQUEST_ID);
            b3Headers.B3TraceId = GetHeader(context, B3HeaderConstants.B3_TRACE_ID);
            b3Headers.B3SpanId = GetHeader(context, B3HeaderConstants.B3_SPAN_ID);
            b3Headers.B3ParentSpanId = GetHeader(context, B3HeaderConstants.B3_PARENT_SPAN_ID);
            b3Headers.B3Sampled = GetHeader(context, B3HeaderConstants.B3_SAMPLED);
            b3Headers.B3Flags = GetHeader(context, B3HeaderConstants.B3_FLAGS);
            b3Headers.OtSpanContext = GetHeader(context, B3HeaderConstants.OT_SPAN_CONTEXT);

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