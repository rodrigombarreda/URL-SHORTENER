namespace UrlShortener.Core.Services
{
    public interface IUrlService
    {
        Task<string> CreateShortUrlAsync(string longUrl);
        Task<string> GetLongUrlAsync(string shortCode);
    }
}
