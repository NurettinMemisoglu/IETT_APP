using IETT_APP.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace IETT_APP.Infrastructure.Persistence.Interceptors
{
    public class AuditInterceptor : SaveChangesInterceptor
    {
        private static readonly TimeZoneInfo TurkeyZone =
            TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");

        private static DateTime NowTR =>
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TurkeyZone);

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;
            if (context == null) return base.SavingChangesAsync(eventData, result, cancellationToken);

            var entries = context.ChangeTracker.Entries()
                .Where(e => e.Entity is BaseEntity<Guid> &&
                            (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted));

            foreach (var entry in entries)
            {
                var entity = (BaseEntity<Guid>)entry.Entity;

                switch (entry.State)
                {
                    case EntityState.Added:
                        entity.CreatedAt = NowTR;
                        entity.UpdatedAt = NowTR;
                        break;

                    case EntityState.Modified:
                        entity.UpdatedAt = NowTR;
                        break;

                    case EntityState.Deleted:
                        entity.IsDeleted = true;
                        entity.DeletedAt = NowTR;
                        entry.State = EntityState.Modified; // soft delete
                        break;
                }
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
