using IETT_APP.Domain.Common;
using IETT_APP.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IETT_APP.Infrastructure.Persistence
{
    public class AppDbContext : IdentityDbContext<User, IdentityRole, string>
    {
        private static readonly TimeZoneInfo TurkeyZone =
            TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }
        public DbSet<Line<Guid>> Lines { get; set; }
        public DbSet<Stop<Guid>> Stops { get; set; }
        public DbSet<Route<Guid>> Routes { get; set; }
        public DbSet<RouteStop<Guid>> RouteStops { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Stop<Guid>>()
                .HasIndex(s => s.Code)
                .IsUnique();

            builder.Entity<Stop<Guid>>()
                .OwnsOne(d => d.Location, loc =>
                {
                    loc.Property(p => p.Latitude).HasPrecision(8, 6);
                    loc.Property(p => p.Longitude).HasPrecision(8, 6);
                });

            builder.Entity<RouteStop<Guid>>()
                .HasKey(ls => new { ls.RouteId, ls.StopId });

            builder.Entity<RouteStop<Guid>>()
                .HasOne(ls => ls.Route)
                .WithMany(l => l.RouteStops)
                .HasForeignKey(ls => ls.RouteId);

            builder.Entity<RouteStop<Guid>>()
                .HasOne(ls => ls.Stop)
                .WithMany(s => s.RouteStops)
                .HasForeignKey(ls => ls.StopId);

            builder.Entity<Route<Guid>>()
                .HasOne(r => r.Line)
                .WithMany(l => l.Routes)
                .HasForeignKey(r => r.LineId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Route<Guid>>()
                .HasIndex(s => s.Code)
                .IsUnique();

            builder.Entity<Line<Guid>>()
                .HasIndex(s => s.Code)
                .IsUnique();
        }

        // === 🕒 SaveChanges Override: CreatedAt / UpdatedAt TR Saatiyle ===
        public override int SaveChanges()
        {
            ApplyAuditInformation();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditInformation();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void ApplyAuditInformation()
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is BaseEntity<Guid> &&
                           (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var entity = (BaseEntity<Guid>)entry.Entity;
                var nowTr = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TurkeyZone);

                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt = nowTr;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entity.UpdatedAt = nowTr;
                }
            }
        }
    }
}
