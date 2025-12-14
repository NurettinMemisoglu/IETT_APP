using IETT_APP.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Security.Claims;

namespace IETT_APP.Infrastructure.Persistence.Interceptors
{
    public class AuditInterceptor : SaveChangesInterceptor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private static readonly TimeZoneInfo TurkeyZone =
            TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");

        public AuditInterceptor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;
            if (context == null) return base.SavingChangesAsync(eventData, result, cancellationToken);

            context.ChangeTracker.DetectChanges();

            // 1. Zaman ve Kullanıcı
            var nowTr = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TurkeyZone);
            var currentUser = GetCurrentUser();

            // 2. Değişenleri Yakala
            var entries = context.ChangeTracker.Entries<IAuditableEntity>()
                .Where(e => e.State == EntityState.Added ||
                            e.State == EntityState.Modified ||
                            e.State == EntityState.Deleted);

            foreach (var entry in entries)
            {
                // Entity'yi interface'e cast ediyoruz (Nesne referansı)
                var entity = entry.Entity;

                switch (entry.State)
                {
                    case EntityState.Added:
                        entity.CreatedAt = nowTr;
                        entity.CreatedBy = currentUser;

                        entity.UpdatedAt = nowTr;
                        entity.UpdatedBy = currentUser;

                        entity.IsActive = true;
                        entity.IsDeleted = false;
                        break;

                    case EntityState.Modified:
                        // --- KESİN ÇÖZÜM BURASI ---
                        // Entry.Property yerine doğrudan nesneye atama yapıyoruz.
                        entity.UpdatedAt = nowTr;
                        entity.UpdatedBy = currentUser;

                        // LOG EKLE:
                        Console.WriteLine($"[INTERCEPTOR] UpdatedBy Atanıyor: {currentUser}");
                        Console.WriteLine($"[INTERCEPTOR] UpdatedAt Atanıyor: {nowTr}");

                        // Created alanlarını koru (Değişmişse bile geri al veya yoksay)
                        entry.Property(x => x.CreatedAt).IsModified = false;
                        entry.Property(x => x.CreatedBy).IsModified = false;

                        // EF Core'a "Bak, bu alanları kesin değiştirdim, sakın atlama" diyoruz.
                        entry.Property(x => x.UpdatedAt).IsModified = true;
                        entry.Property(x => x.UpdatedBy).IsModified = true;
                        break;

                    case EntityState.Deleted:
                        // Soft Delete
                        entry.State = EntityState.Modified;

                        entity.IsDeleted = true;
                        entity.DeletedAt = nowTr;
                        entity.DeletedBy = currentUser;

                        // EF Core'a bu alanların değiştiğini bildir
                        entry.Property(x => x.IsDeleted).IsModified = true;
                        entry.Property(x => x.DeletedAt).IsModified = true;
                        entry.Property(x => x.DeletedBy).IsModified = true;
                        break;
                }
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private string GetCurrentUser()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
                return "System";

            // İsim, Email veya NameIdentifier bulmaya çalış
            return user.FindFirst(ClaimTypes.Email)?.Value
                   ?? user.FindFirst("email")?.Value
                   ?? user.Identity.Name
                   ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? "System";
        }
    }
}