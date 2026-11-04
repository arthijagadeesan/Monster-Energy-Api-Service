using Microsoft.EntityFrameworkCore;
using MonsterEnergy.Domain.Entities;

namespace MonsterEnergy.Infrastructure.Database
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Store> Stores => Set<Store>();
        public DbSet<UserFavourite> UserFavourites => Set<UserFavourite>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
