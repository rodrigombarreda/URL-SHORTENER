using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace UrlShortener.Infrastructure.Data
{
    public class UrlShortenerDbContextFactory : IDesignTimeDbContextFactory<UrlShortenerDbContext>
    {
        public UrlShortenerDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<UrlShortenerDbContext>();
            optionsBuilder.UseSqlServer("Server=(localdb)\\LocalDB;Database=UrlShortener;Trusted_Connection=true;TrustServerCertificate=true;");
            
            return new UrlShortenerDbContext(optionsBuilder.Options);
        }
    }
}
