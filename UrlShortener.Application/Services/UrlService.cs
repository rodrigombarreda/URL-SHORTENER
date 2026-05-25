using UrlShortener.Core.Entities;
using UrlShortener.Core.Repositories;
using UrlShortener.Core.Services;
using Microsoft.Extensions.Logging;
using Prometheus;

namespace UrlShortener.Application.Services
{
    public class UrlService : IUrlService
    {
        private readonly IUrlRepository _urlRepository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<UrlService> _logger;

        // Contadores Prometheus
        private static readonly Counter UrlsCreated = Metrics.CreateCounter(
            "urls_created_total", "Total de URLs creadas");

        private static readonly Counter UrlsRedirected = Metrics.CreateCounter(
            "urls_redirected_total", "Total de redirecciones");

        public UrlService(IUrlRepository urlRepository, ICacheService cacheService, ILogger<UrlService> logger)
        {
            _urlRepository = urlRepository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<string> CreateShortUrlAsync(string longUrl)
        {
            var cachedShortCode = await _cacheService.GetAsync($"longurl:{longUrl}");
            if (!string.IsNullOrEmpty(cachedShortCode))
                return cachedShortCode;

            var existing = await _urlRepository.GetByLongUrlAsync(longUrl);
            if (existing != null)
            {
                await CacheUrlMappingAsync(existing.LongUrl, existing.ShortCode!);
                return existing.ShortCode!;
            }

            var urlTable = new UrlTable
            {
                LongUrl = longUrl,
                CreatedOnUtc = DateTime.UtcNow
            };

            string shortCode = await _urlRepository.CreateAsync(urlTable);

            await CacheUrlMappingAsync(longUrl, shortCode);

            _logger.LogInformation("ID: {Id} - ShortCode: {ShortCode}", urlTable.Id, shortCode);
            UrlsCreated.Inc();

            return shortCode;
        }

        private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(10);

        private Task CacheUrlMappingAsync(string longUrl, string shortCode) =>
            Task.WhenAll(
                _cacheService.SetAsync($"longurl:{longUrl}", shortCode, CacheTtl),
                _cacheService.SetAsync($"url:{shortCode}", longUrl, CacheTtl)
            );

        public async Task<string> GetLongUrlAsync(string shortCode)
        {
            if (string.IsNullOrWhiteSpace(shortCode))
                throw new ArgumentException("ShortCode cannot be empty");

            try
            {
                var cachedUrl = await _cacheService.GetAsync($"url:{shortCode}");
                if (!string.IsNullOrEmpty(cachedUrl))
                {
                    UrlsRedirected.Inc();
                    return cachedUrl;
                }

                var urlTable = await _urlRepository.GetByShortCodeAsync(shortCode)
                    ?? throw new KeyNotFoundException($"No URL found for shortcode {shortCode}");

                await _cacheService.SetAsync($"url:{shortCode}", urlTable.LongUrl, CacheTtl);
                UrlsRedirected.Inc();

                return urlTable.LongUrl;
            }
            catch (Exception ex) when (ex is not KeyNotFoundException && ex is not ArgumentException)
            {
                _logger.LogError(ex, "Unexpected error retrieving long URL for {ShortCode}", shortCode);
                throw;
            }
        }
    }
}
