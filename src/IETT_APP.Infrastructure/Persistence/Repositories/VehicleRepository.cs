using IETT_APP.Domain.Entities;
using IETT_APP.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IETT_APP.Infrastructure.Persistence.Repositories
{
    public class VehicleRepository<T> : IVehicleRepository<T>
    {
        private readonly AppDbContext _context;
        private readonly DbSet<Vehicle<T>> _dbSet;
        private static readonly TimeZoneInfo TurkeyZone =
            TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");

        public VehicleRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<Vehicle<T>>();
        }

        public async Task<IEnumerable<Vehicle<T>>> GetAllAsync() =>
            await _dbSet
            .Include(v => v.Garage)
            .OrderBy(v => v.CreatedAt)
            .ToListAsync();

        public async Task<Vehicle<T>?> GetByIdAsync(T id) =>
            await _dbSet
                .Include(v => v.Garage)
                .FirstOrDefaultAsync(v => v.Id!.Equals(id));

        public async Task AddAsync(Vehicle<T> vehicle)
        {
            var exists = await _context.Set<Vehicle<T>>()
                     .AnyAsync(x => x.PlateNumber == vehicle.PlateNumber && !x.IsDeleted);

            if (exists)
                throw new ArgumentException($"Aynı güzergah kodu ile aktif bir hat mevcut: {vehicle.PlateNumber}");
            // 🇹🇷 TR saatini kullan
            var nowTr = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TurkeyZone);

            vehicle.CreatedAt = nowTr;
            vehicle.CreatedBy = "System";

            await _dbSet.AddAsync(vehicle);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Vehicle<T> vehicle)
        {
            var existing = await _dbSet.AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id!.Equals(vehicle.Id));

            if (existing == null)
                throw new Exception("Güncellenecek araç bulunamadı.");

            var nowTr = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TurkeyZone);

            // CreatedAt/By sabit kalsın
            vehicle.CreatedAt = existing.CreatedAt;
            vehicle.CreatedBy = existing.CreatedBy;

            // UpdatedAt/By güncellensin
            vehicle.UpdatedAt = nowTr;
            vehicle.UpdatedBy = "System";

            // EF'e sadece değişen property'leri işaretle
            _context.Entry(vehicle).State = EntityState.Modified;
            _context.Entry(vehicle).Property(x => x.CreatedAt).IsModified = false;
            _context.Entry(vehicle).Property(x => x.CreatedBy).IsModified = false;

            await _context.SaveChangesAsync();
        }


        public async Task DeleteAsync(T id)
        {
            var vehicle = await _context.Set<Vehicle<T>>().FirstOrDefaultAsync(x => x.Id!.Equals(id));
            if (vehicle == null) return;

            var nowTr = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TurkeyZone);

            vehicle.IsDeleted = true;
            vehicle.DeletedAt = nowTr;
            vehicle.DeletedBy = "System";

            _context.Set<Vehicle<T>>().Update(vehicle);
            await _context.SaveChangesAsync();
        }
    }
}
