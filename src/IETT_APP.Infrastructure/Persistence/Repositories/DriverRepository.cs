using IETT_APP.Domain.Entities;
using IETT_APP.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage; // Transaction için gerekli

namespace IETT_APP.Infrastructure.Persistence.Repositories
{
    public class DriverRepository : IDriverRepository
    {
        private readonly AppDbContext _context;

        public DriverRepository(AppDbContext context)
        {
            _context = context;
        }

        // --- OKUMA İŞLEMLERİ ---
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
                .Include(d => d.User) // User tablosunu dahil et
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
            _context.Entry(entity).State = EntityState.Added;

            // Not: Bu SaveChanges, Transaction commit edilene kadar
            // veritabanında kalıcı olmaz. Güvenlidir.
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Driver entity)
        {
            if (_context.Entry(entity).State == EntityState.Detached)
            {
                _context.Drivers.Attach(entity);
            }
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteAsync(Driver entity)
        {
            _context.Drivers.Remove(entity);
            await _context.SaveChangesAsync();
        }

        // ✅ YENİ EKLENEN: Transaction Yönetimi
        // Service katmanı artık _context çağırmadan buradan transaction başlatabilir.
        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }
    }
}