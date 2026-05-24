using Microsoft.Extensions.DependencyInjection;
using UrlShortener.Core.Services;
using UrlShortener.Application.Services;

namespace UrlShortener.Application.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IUrlService, UrlService>();
            services.AddScoped<ICacheService, RedisCacheService>();
            return services;
        }
    }
}
