using IETT_APP.Domain.Entities;
using IETT_APP.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IETT_APP.Infrastructure.Persistence.Repositories
{
    public class DriverRepository : IDriverRepository
    {
        private readonly AppDbContext _context;

        public DriverRepository(AppDbContext context)
        {
            _context = context;
        }

        // --- OKUMA İŞLEMLERİ (AsNoTracking opsiyonel performans için eklenebilir) ---

        public async Task<IEnumerable<Driver>> GetAllWithDetailsAsync()
        {
            return await _context.Drivers
                .Include(d => d.User)
                .Include(d => d.Garage)
                .ToListAsync();
        }

        public async Task<Driver?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _context.Drivers
                .Include(d => d.User)
                .Include(d => d.Garage)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<Driver?> GetByUserIdAsync(string userId)
        {
            return await _context.Drivers
                .Include(d => d.User)
                .Include(d => d.Garage)
                .FirstOrDefaultAsync(d => d.UserId == userId);
        }

        public async Task<IEnumerable<Driver>> GetUnassignedDriversAsync()
        {
            return await _context.Drivers
                .Include(d => d.User)
                .Where(d => d.GarageId == null)
                .ToListAsync();
        }

        // --- YAZMA İŞLEMLERİ ---

        public async Task AddAsync(Driver entity)
        {
            await _context.Drivers.AddAsync(entity);

            // EntityState.Added olduğundan emin olmak için (Interceptor için)
            _context.Entry(entity).State = EntityState.Added;

            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Driver entity)
        {
            // Eğer entity zaten context tarafından takip ediliyorsa (Tracked), direkt state değiştir.
            // Değilse Attach et.
            if (_context.Entry(entity).State == EntityState.Detached)
            {
                _context.Drivers.Attach(entity);
            }

            _context.Entry(entity).State = EntityState.Modified;

            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteAsync(Driver entity)
        {
            // Interceptor'ın "Deleted" state'ini yakalayıp "Modified" (IsDeleted=true) yapması için
            // Remove metodunu çağırıyoruz.
            _context.Drivers.Remove(entity);
            await _context.SaveChangesAsync();
        }


    }
}