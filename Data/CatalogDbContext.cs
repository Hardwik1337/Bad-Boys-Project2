using Microsoft.EntityFrameworkCore;
using MovieHub.Models;

namespace MovieHub.Data
{
    public class CatalogDbContext : DbContext
    {
        public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
            : base(options)
        {
        }

        public DbSet<Movie> Movies { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<StreamingProvider> StreamingProviders { get; set; }
    }
}