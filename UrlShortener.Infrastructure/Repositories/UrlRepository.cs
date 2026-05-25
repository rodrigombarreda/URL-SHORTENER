using Microsoft.Data.SqlClient;
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

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.UrlTables.Add(urlTable);
                await _context.SaveChangesAsync();

                urlTable.ShortCode = Base62Converter.Encode(urlTable.Id);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return urlTable.ShortCode;
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                await transaction.RollbackAsync();
                var existing = await _context.UrlTables
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.LongUrl == urlTable.LongUrl);
                return existing!.ShortCode!;
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Database error while creating URL {LongUrl}", urlTable.LongUrl);
                throw new ApplicationException("Error creating URL in database", ex);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private static bool IsUniqueConstraintViolation(DbUpdateException ex) =>
            ex.InnerException is SqlException sqlEx && (sqlEx.Number == 2601 || sqlEx.Number == 2627);

        public async Task<UrlTable?> GetByShortCodeAsync(string shortCode)
        {
            if (string.IsNullOrWhiteSpace(shortCode))
                throw new ArgumentException("ShortCode is required");

            return await _context.UrlTables
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.ShortCode == shortCode);
        }

        public async Task<UrlTable?> GetByLongUrlAsync(string longUrl)
        {
            if (string.IsNullOrWhiteSpace(longUrl))
                throw new ArgumentException("LongUrl is required");

            return await _context.UrlTables
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.LongUrl == longUrl);
        }

    }
}
