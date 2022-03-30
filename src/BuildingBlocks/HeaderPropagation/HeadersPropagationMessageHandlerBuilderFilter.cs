using HeaderPropagation.B3Headers;
using HeaderPropagation.TenantHeaders;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace HeaderPropagation
{
	public class HeadersPropagationMessageHandlerBuilderFilter : IHttpMessageHandlerBuilderFilter
	{
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly ILogger<B3HeadersPropagationDelegatingHandler> _delegatingB3HeaderHandlerLogger;
        private readonly ILogger<TenantHeadersPropagationDelegatingHandler> _delegatingTenantHeaderHandlerLogger;

        public HeadersPropagationMessageHandlerBuilderFilter(IHttpContextAccessor httpContextAccessor,
            ILogger<B3HeadersPropagationDelegatingHandler> delegatingB3HeaderHandlerLogger,
            ILogger<TenantHeadersPropagationDelegatingHandler> delegatingTenantHeaderHandlerLogger)
        {
            this._httpContextAccessor = httpContextAccessor;
            this._delegatingB3HeaderHandlerLogger = delegatingB3HeaderHandlerLogger ?? throw new ArgumentNullException(nameof(delegatingB3HeaderHandlerLogger));
            this._delegatingTenantHeaderHandlerLogger = delegatingTenantHeaderHandlerLogger ?? throw new ArgumentNullException(nameof(delegatingTenantHeaderHandlerLogger));
		}

        public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            return (builder) =>
            {
                next(builder);
                builder.AdditionalHandlers.Add(new B3HeadersPropagationDelegatingHandler(_httpContextAccessor, _delegatingB3HeaderHandlerLogger));
                builder.AdditionalHandlers.Add(new TenantHeadersPropagationDelegatingHandler(_httpContextAccessor, _delegatingTenantHeaderHandlerLogger));
            };
        }
    }
}