using HeaderPropagation.B3Headers;
using HeaderPropagation.B3Headers.Interfaces;
using HeaderPropagation.TenantHeaders;
using HeaderPropagation.TenantHeaders.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;

namespace HeaderPropagation
{
    public static class ConfigureHeaderPropagationMiddlewareExtensionsExtensions
    {
        public static IServiceCollection AddHeaderPropagationMiddleware(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();


            services.TryAddScoped<IB3HeadersHolder, B3HeadersHolder>();
            services.TryAddScoped<ITenantHeadersHolder, TenantHeadersHolder>();

            services.TryAddTransient<B3HeadersPropagationDelegatingHandler>();
            services.TryAddTransient<TenantHeadersPropagationDelegatingHandler>();

            services.AddSingleton<IHttpMessageHandlerBuilderFilter, HeadersPropagationMessageHandlerBuilderFilter>();

            return services;
        }

        public static IApplicationBuilder UseB3HeaderPropagationMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<B3HeadersFetcherMiddleware>();
        }

        public static IApplicationBuilder UseTenantHeaderPropagationMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TenantHeadersFetcherMiddleware>();
        }
    }
}