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
        public DbSet<Garage<Guid>> Garages { get; set; }
        public DbSet<Vehicle<Guid>> Vehicles { get; set; }
        public DbSet<TripTask> TripTasks { get; set; }
        public DbSet<TripTaskHistory> TripTaskHistories { get; set; }
        public DbSet<Operator> Operators { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Query filters
            builder.Entity<Vehicle<Guid>>().HasQueryFilter(v => !v.IsDeleted);
            builder.Entity<Garage<Guid>>().HasQueryFilter(g => !g.IsDeleted);
            builder.Entity<TripTask>().HasQueryFilter(t => !t.IsDeleted);

            // Stop index
            builder.Entity<Stop<Guid>>()
                .HasIndex(s => s.Code)
                .IsUnique();

            // Stop location precision
            builder.Entity<Stop<Guid>>()
                .OwnsOne(d => d.Location, loc =>
                {
                    loc.Property(p => p.Latitude).HasPrecision(8, 6);
                    loc.Property(p => p.Longitude).HasPrecision(8, 6);
                });

            // RouteStop composite key
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

            // Route -> Line
            builder.Entity<Route<Guid>>()
                .HasOne(r => r.Line)
                .WithMany(l => l.Routes)
                .HasForeignKey(r => r.LineId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Route<Guid>>()
                .HasIndex(s => s.Code)
                .IsUnique();

            // Line index
            builder.Entity<Line<Guid>>()
                .HasIndex(s => s.Code)
                .IsUnique();

            // === TripTask ===
            builder.Entity<TripTask>(entity =>
            {
                // Indexler
                entity.HasIndex(t => t.VehicleId);
                entity.HasIndex(t => t.OperatorId);
                entity.HasIndex(t => t.LineId);
                entity.HasIndex(t => t.RouteId);
                entity.HasIndex(t => t.GarageId);

                // Foreign key ilişkileri
                entity.HasOne(t => t.Vehicle)
                      .WithMany(v => v.TripTasks)
                      .HasForeignKey(t => t.VehicleId);

                entity.HasOne(t => t.Operator)
                      .WithMany(o => o.TripTasks)
                      .HasForeignKey(t => t.OperatorId);

                entity.HasOne(t => t.Line)
                      .WithMany(l => l.TripTasks)
                      .HasForeignKey(t => t.LineId);

                entity.HasOne(t => t.Route)
                      .WithMany(r => r.TripTasks)
                      .HasForeignKey(t => t.RouteId);

                entity.HasOne(t => t.Garage)
                      .WithMany(g => g.TripTasks)
                      .HasForeignKey(t => t.GarageId);
            });
        }


        // === SaveChanges Override: CreatedAt / UpdatedAt TR Saatiyle ===
        public override int SaveChanges()
        {

            ApplyAuditInformation();
            ApplyEntityHistory();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditInformation();
            ApplyEntityHistory();
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


        /// TripTask güncellemelerinde değişen alanları otomatik olarak TripTaskHistory tablosuna kaydeder.

        private void ApplyEntityHistory()
        {
            var nowTr = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TurkeyZone);

            var excludedProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Id", "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy",
                "DeletedAt", "DeletedBy", "IsDeleted", "TripTaskHistories"
            };

            var modifiedEntries = ChangeTracker.Entries<TripTask>()
                .Where(e => e.State == EntityState.Modified);

            var histories = new List<TripTaskHistory>();

            foreach (var entry in modifiedEntries)
            {
                var original = entry.OriginalValues;
                var current = entry.CurrentValues;
                var entityId = entry.Entity.Id;

                foreach (var prop in entry.Properties)
                {
                    var propName = prop.Metadata.Name;
                    if (excludedProperties.Contains(propName) || prop.Metadata.IsShadowProperty())
                        continue;

                    var oldVal = original[propName]?.ToString();
                    var newVal = current[propName]?.ToString();

                    if (string.Equals(oldVal ?? string.Empty, newVal ?? string.Empty, StringComparison.Ordinal))
                        continue;

                    histories.Add(new TripTaskHistory
                    {
                        Id = Guid.NewGuid(),
                        TripTaskId = entityId,
                        FieldName = propName,
                        OldValue = oldVal ?? string.Empty,
                        NewValue = newVal ?? string.Empty,
                        CreatedAt = nowTr
                    });
                }
            }

            if (histories.Any())
            {
                // Burada CreatedAt’a göre sırala: en yeni en üstte
                var sorted = histories.OrderByDescending(h => h.CreatedAt).ToList();
                TripTaskHistories.AddRange(sorted);
            }
        }
    }
}


