using Microsoft.Extensions.DependencyInjection;
using UrlShortener.Core.Repositories;
using UrlShortener.Infrastructure.Repositories;
using UrlShortener.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace UrlShortener.Infrastructure.Extensions
{
    public static class InfrastructureServicesExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration config)
        {
            var redisConnection = config["ConnectionStrings:Redis"] ?? "redis:6379";
            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnection));

            services.AddDbContext<UrlShortenerDbContext>(options =>
                options.UseSqlServer(
                    config.GetConnectionString("DefaultConnection")));

            services.AddScoped<IUrlRepository, UrlRepository>();
            return services;
        }
    }
}
