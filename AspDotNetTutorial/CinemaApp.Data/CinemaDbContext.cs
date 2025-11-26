using CinemaApp.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;


namespace CinemaApp.Data
{
    public class CinemaDbContext : DbContext
    {
        // Constructor: Passes configuration (like connection strings) to the base class
        public CinemaDbContext(DbContextOptions<CinemaDbContext> options) : base(options)
        {
        }

        // These properties act as tables in your database
        public DbSet<Movie> Movies { get; set; }
        public DbSet<Showtime> Showtimes { get; set; }

        // Logic to automate LastModified
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<Movie>())
            {
                if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                {
                    entry.Entity.LastModified = DateTime.UtcNow;
                }
            }

            foreach (var entry in ChangeTracker.Entries<Showtime>())
            {
                if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                {
                    entry.Entity.LastModified = DateTime.UtcNow;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
