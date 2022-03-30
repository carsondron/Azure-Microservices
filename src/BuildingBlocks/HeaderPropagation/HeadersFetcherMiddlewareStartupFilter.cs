using HeaderPropagation.B3Headers;
using HeaderPropagation.TenantHeaders;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace HeaderPropagation
{
	public class HeadersFetcherMiddlewareStartupFilter : IStartupFilter
	{
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return (builder) =>
            {
                builder.UseMiddleware<B3HeadersFetcherMiddleware>();
                builder.UseMiddleware<TenantHeadersFetcherMiddleware>();
                next(builder);
            };
        }
    }
}