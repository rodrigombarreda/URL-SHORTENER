using UrlShortener.Core.Entities;

namespace UrlShortener.Core.Repositories
{
    public interface IUrlRepository
    {
        Task<string> CreateAsync(UrlTable urlTable);
        Task<UrlTable?> GetByShortCodeAsync(string shortCode);
        Task<UrlTable?> GetByLongUrlAsync(string longUrl);
    }
}
