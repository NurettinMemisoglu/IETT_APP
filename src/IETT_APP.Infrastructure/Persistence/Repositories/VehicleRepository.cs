using IETT_APP.Application.Interfaces;
using IETT_APP.Domain.Entities;
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
            // 🇹🇷 TR saatini kullan
            var nowTr = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TurkeyZone);

            vehicle.CreatedAt = nowTr;
            vehicle.CreatedBy = "System";

            await _dbSet.AddAsync(vehicle);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Vehicle<T> vehicle)
        {
            var nowTr = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TurkeyZone);

            vehicle.UpdatedAt = nowTr;
            vehicle.UpdatedBy = "System";

            _dbSet.Update(vehicle);
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
