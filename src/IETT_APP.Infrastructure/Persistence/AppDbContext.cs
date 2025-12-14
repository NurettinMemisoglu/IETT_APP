using IETT_APP.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IETT_APP.Infrastructure.Persistence
{
    public class AppDbContext : IdentityDbContext<User, IdentityRole, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        // === DB Sets ===
        public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }
        public DbSet<Line<Guid>> Lines { get; set; }
        public DbSet<Stop<Guid>> Stops { get; set; }
        public DbSet<Route<Guid>> Routes { get; set; }
        public DbSet<RouteStop<Guid>> RouteStops { get; set; }
        public DbSet<Garage<Guid>> Garages { get; set; }
        public DbSet<Vehicle<Guid>> Vehicles { get; set; }
        public DbSet<TripTask> TripTasks { get; set; }
        public DbSet<TripTaskHistory> TripTaskHistories { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<FileRecorder> FileRecorders { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ==================================================================================
            // 1. GLOBAL QUERY FILTERS (Soft Delete)
            // ==================================================================================
            builder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
            builder.Entity<Vehicle<Guid>>().HasQueryFilter(v => !v.IsDeleted);
            builder.Entity<Garage<Guid>>().HasQueryFilter(g => !g.IsDeleted);
            builder.Entity<TripTask>().HasQueryFilter(t => !t.IsDeleted);

            // --- YENİLER İÇİN FİLTRE ---
            builder.Entity<Driver>().HasQueryFilter(d => !d.IsDeleted);
            builder.Entity<FileRecorder>().HasQueryFilter(f => !f.IsDeleted);

            // ==================================================================================
            // 2. DRIVER (OPERATOR) CONFIGURATION
            // ==================================================================================
            builder.Entity<Driver>(entity =>
            {
                entity.HasOne(d => d.User)
                      .WithOne(u => u.Driver)
                      .HasForeignKey<Driver>(d => d.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Garage)
                      .WithMany()
                      .HasForeignKey(d => d.GarageId)
                      .OnDelete(DeleteBehavior.Restrict);

                // 1. Sicil No Benzersizliği (Sadece Silinmemişlerde)
                entity.HasIndex(d => d.EmployeeNumber)
                      .IsUnique()
                      .HasFilter("[IsDeleted] = 0"); // SQL Server Filtresi

                // 2. TC Kimlik Benzersizliği (Sadece Silinmemişlerde)
                // Not: TCIdentityNumber nullable ise HasFilter içine "[TCIdentityNumber] IS NOT NULL AND [IsDeleted] = 0" da eklenebilir ama genelde Required olduğu için gerekmez.
                entity.HasIndex(d => d.TCIdentityNumber)
                      .IsUnique()
                      .HasFilter("[IsDeleted] = 0");
            });

            // ==================================================================================
            // 3. TRIP TASK CONFIGURATION
            // ==================================================================================
            builder.Entity<TripTask>(entity =>
            {
                entity.HasIndex(t => t.VehicleId);
                entity.HasIndex(t => t.DriverId); // OperatorId -> DriverId oldu
                entity.HasIndex(t => t.LineId);
                entity.HasIndex(t => t.RouteId);
                entity.HasIndex(t => t.GarageId);

                entity.HasOne(t => t.Vehicle)
                      .WithMany(v => v.TripTasks)
                      .HasForeignKey(t => t.VehicleId);

                // Driver silinirse geçmiş görevler silinmesin (Restrict)
                entity.HasOne(t => t.Driver)
                      .WithMany(d => d.TripTasks)
                      .HasForeignKey(t => t.DriverId)
                      .OnDelete(DeleteBehavior.Restrict);

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

            // ==================================================================================
            // 4. OTHER CONFIGURATIONS
            // ==================================================================================
            builder.Entity<Stop<Guid>>().HasIndex(s => s.Code).IsUnique();
            builder.Entity<Stop<Guid>>().OwnsOne(d => d.Location, loc =>
            {
                loc.Property(p => p.Latitude).HasPrecision(8, 6);
                loc.Property(p => p.Longitude).HasPrecision(8, 6);
            });

            builder.Entity<RouteStop<Guid>>().HasKey(ls => new { ls.RouteId, ls.StopId });

            builder.Entity<RouteStop<Guid>>().HasOne(ls => ls.Route).WithMany(l => l.RouteStops).HasForeignKey(ls => ls.RouteId);
            builder.Entity<RouteStop<Guid>>().HasOne(ls => ls.Stop).WithMany(s => s.RouteStops).HasForeignKey(ls => ls.StopId);

            builder.Entity<Route<Guid>>().HasOne(r => r.Line).WithMany(l => l.Routes).HasForeignKey(r => r.LineId).OnDelete(DeleteBehavior.Cascade);
            builder.Entity<Route<Guid>>().HasIndex(s => s.Code).IsUnique();
            builder.Entity<Line<Guid>>().HasIndex(s => s.Code).IsUnique();

            // ==================================================================================
            // 5. ASP.NET IDENTITY USER CONFIGURATION (Soft Delete Fix)
            // ==================================================================================

            // Identity'nin varsayılan UserNameIndex'ini ve EmailIndex'ini filtreli hale getiriyoruz.
            // Böylece IsDeleted=1 olan (silinmiş) kullanıcıların e-postaları tekrar kullanılabilir olur.

            builder.Entity<User>()
                .HasIndex(u => u.NormalizedUserName)
                .HasDatabaseName("UserNameIndex") // Varsayılan index adı
                .IsUnique()
                .HasFilter("[IsDeleted] = 0"); // Sadece silinmemişler benzersiz olsun

            builder.Entity<User>()
                .HasIndex(u => u.NormalizedEmail)
                .HasDatabaseName("EmailIndex") // Varsayılan index adı
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            // ==================================================================================
            // SOFT DELETE İLİŞKİ DÜZELTMELERİ (Warning 10622 Çözümü)
            // ==================================================================================

            // 1. Notification -> User İlişkisi
            builder.Entity<Notification>(entity =>
            {
                entity.HasOne(n => n.User)
                      .WithMany()
                      .HasForeignKey(n => n.UserId)
                      .IsRequired(false); // <-- KRİTİK NOKTA: İlişkiyi opsiyonel yapıyoruz
            });

            // 2. UserRefreshToken -> User İlişkisi
            builder.Entity<UserRefreshToken>(entity =>
            {
                entity.HasOne(t => t.User)
                      .WithMany()
                      .HasForeignKey(t => t.UserId)
                      .IsRequired(false); // <-- KRİTİK NOKTA
            });

            // 3. Driver -> User İlişkisi (Bunu da eklemekte fayda var)
            builder.Entity<Driver>(entity =>
            {
                entity.HasOne(d => d.User)
                      .WithOne(u => u.Driver)
                      .HasForeignKey<Driver>(d => d.UserId)
                      .IsRequired(false); // <-- KRİTİK NOKTA
            });
        }
    }
}