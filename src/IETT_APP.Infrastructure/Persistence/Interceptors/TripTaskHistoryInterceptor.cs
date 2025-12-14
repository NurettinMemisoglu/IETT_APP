// IETT_APP.Infrastructure/Persistence/Interceptors/TripTaskHistoryInterceptor.cs
using IETT_APP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace IETT_APP.Infrastructure.Persistence.Interceptors
{
    public class TripTaskHistoryInterceptor : SaveChangesInterceptor
    {
        private static readonly TimeZoneInfo TurkeyZone =
            TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;
            if (context == null) return base.SavingChangesAsync(eventData, result, cancellationToken);

            var nowTr = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TurkeyZone);

            // Sadece Modified olan TripTask'leri yakala
            var modifiedEntries = context.ChangeTracker.Entries<TripTask>()
                .Where(e => e.State == EntityState.Modified)
                .ToList();

            if (!modifiedEntries.Any())
                return base.SavingChangesAsync(eventData, result, cancellationToken);

            var excludedProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Id", "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy",
                "DeletedAt", "DeletedBy", "IsDeleted", "TripTaskHistories"
            };

            var historiesToAdd = new List<TripTaskHistory>();

            foreach (var entry in modifiedEntries)
            {
                var original = entry.OriginalValues;
                var current = entry.CurrentValues;
                var entityId = entry.Entity.Id;

                foreach (var prop in entry.Properties)
                {
                    var propName = prop.Metadata.Name;

                    // Shadow property veya yasaklı alan ise geç
                    if (excludedProperties.Contains(propName) || prop.Metadata.IsShadowProperty())
                        continue;

                    var oldVal = original[propName]?.ToString();
                    var newVal = current[propName]?.ToString();

                    // Değer değişmediyse geç
                    if (string.Equals(oldVal ?? string.Empty, newVal ?? string.Empty, StringComparison.Ordinal))
                        continue;

                    historiesToAdd.Add(new TripTaskHistory
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

            if (historiesToAdd.Any())
            {
                context.Set<TripTaskHistory>().AddRange(historiesToAdd);
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}