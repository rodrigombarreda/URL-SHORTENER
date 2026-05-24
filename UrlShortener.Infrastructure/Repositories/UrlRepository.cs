using Microsoft.EntityFrameworkCore;
using UrlShortener.Core.Entities;
using UrlShortener.Core.Repositories;
using UrlShortener.Infrastructure.Data;
using UrlShortener.Infrastructure.Utilities;
using Microsoft.Extensions.Logging;

namespace UrlShortener.Infrastructure.Repositories
{
    public class UrlRepository : IUrlRepository
    {
        private readonly UrlShortenerDbContext _context;
        private readonly ILogger<UrlRepository> _logger;

        public UrlRepository(UrlShortenerDbContext context, ILogger<UrlRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<string> CreateAsync(UrlTable urlTable)
        {
            if (string.IsNullOrWhiteSpace(urlTable.LongUrl))
                throw new ArgumentException("LongUrl is required");

            try
            {
                _context.UrlTables.Add(urlTable);
                await _context.SaveChangesAsync();

                urlTable.ShortCode = Base62Converter.Encode(urlTable.Id);
                await _context.SaveChangesAsync();

                return urlTable.ShortCode;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database update error while creating URL {LongUrl}", urlTable.LongUrl);
                throw new ApplicationException("Error creating URL in database", ex);
            }
        }

        public async Task<UrlTable> GetByShortCodeAsync(string shortCode)
        {
            if (string.IsNullOrWhiteSpace(shortCode))
                throw new ArgumentException("ShortCode is required");

            var result = await _context.UrlTables
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.ShortCode == shortCode);

            if (result == null)
                throw new KeyNotFoundException($"ShortCode {shortCode} not found");

            return result;
        }

    }
}
