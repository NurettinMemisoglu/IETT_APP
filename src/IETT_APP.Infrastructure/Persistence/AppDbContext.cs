using IETT_APP.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IETT_APP.Infrastructure.Persistence
{
    public class AppDbContext : IdentityDbContext<User, IdentityRole, string>
    {

        public DbSet<Stop> Stops { get; set; } = null!;
        public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }  // ← Burayı ekle
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }



        protected override void OnModelCreating(ModelBuilder builder)
        {


            base.OnModelCreating(builder);


            builder.Entity<Stop>()
                .HasIndex(s => s.Code)
                .IsUnique();

            builder.Entity<Stop>()
                .OwnsOne(d => d.Location);

            builder.Entity<Stop>()
                .OwnsOne(s => s.Location, loc =>
                {
                    loc.Property(p => p.Latitude).HasPrecision(8, 6);
                    loc.Property(p => p.Longitude).HasPrecision(8, 6);
                });

        }
    }
}
