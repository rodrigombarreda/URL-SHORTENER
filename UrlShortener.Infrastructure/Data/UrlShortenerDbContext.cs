using Microsoft.EntityFrameworkCore;
using UrlShortener.Core.Entities;

namespace UrlShortener.Infrastructure.Data
{
    public class UrlShortenerDbContext : DbContext
    {
        public UrlShortenerDbContext(DbContextOptions<UrlShortenerDbContext> options) : base(options)
        {
        }

        public DbSet<UrlTable> UrlTables { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UrlTable>()
                .HasKey(u => u.Id);

            modelBuilder.Entity<UrlTable>()
                .Property(u => u.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<UrlTable>()
                .HasIndex(u => u.ShortCode)
                .IsUnique();

            modelBuilder.Entity<UrlTable>()
                .Property(u => u.ShortCode)
                .HasMaxLength(20)
                .UseCollation("Latin1_General_100_BIN2");

            modelBuilder.Entity<UrlTable>()
                .HasIndex(u => u.LongUrl);
        }
    }
}
