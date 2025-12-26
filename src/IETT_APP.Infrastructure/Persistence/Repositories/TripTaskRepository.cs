using IETT_APP.Domain.Entities;
using IETT_APP.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IETT_APP.Infrastructure.Persistence.Repositories
{
    public class TripTaskRepository : ITripTaskRepository
    {
        private readonly AppDbContext _context;

        public TripTaskRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(TripTask tripTask)
        {
            await _context.TripTasks.AddAsync(tripTask);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<TripTask>> GetAllAsync(string? creatorName = null)
        {
            // 1. Sorguyu başlat (Henüz DB'ye gitmedi)
            var query = _context.TripTasks.AsQueryable();

            // 2. Temel Filtre (Silinmemişler)
            query = query.Where(t => !t.IsDeleted);

            // 3. EĞER YARATICI İSMİ GELDİYSE FİLTRELE (Kritik Nokta)
            if (!string.IsNullOrEmpty(creatorName))
            {
                query = query.Where(t => t.CreatedBy == creatorName);
            }

            // 4. Include işlemleri ve Sıralama
            return await query
                .Include(t => t.Vehicle)
                .Include(t => t.Driver)
                    .ThenInclude(d => d.User)
                .Include(t => t.Line)
                .Include(t => t.Route)
                .Include(t => t.Garage)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync(); // 5. Sorguyu çalıştır ve sonucu dön
        }

        public async Task<IEnumerable<TripTask>> GetByDriverIdAsync(Guid driverId)
        {
            return await _context.TripTasks
                .Where(t => t.DriverId == driverId && !t.IsDeleted) // Sadece bu şoförün aktif görevleri
                .Include(t => t.Vehicle)  // Araç bilgisini getir
                .Include(t => t.Line)     // Hat bilgisini getir
                .Include(t => t.Route)    // Güzergah bilgisini getir
                .Include(t => t.Garage)   // Garaj bilgisini getir
                .OrderByDescending(t => t.ScheduledDeparture) // En yakın/yeni görev en üstte olsun
                .ToListAsync();
        }

        public async Task<TripTask?> GetByIdAsync(Guid id)
        {
            return await _context.TripTasks
                .Include(t => t.TripTaskHistories)
                .Include(t => t.Line)       // Hat Kodu için şart
                .Include(t => t.Route)      // Güzergah için şart
                .Include(t => t.Vehicle)    // Plaka için şart
                .Include(t => t.Garage)     // Garaj adı için şart
                .Include(t => t.Driver)
                    .ThenInclude(d => d.User) // Şoför adı ve maili için şart
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
        }

        public async Task UpdateAsync(TripTask tripTask)
        {
            _context.TripTasks.Update(tripTask);
            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteAsync(TripTask tripTask, string? reason = null)
        {
            _context.TripTasks.Remove(tripTask);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.TripTasks.AnyAsync(t => t.Id == id && !t.IsDeleted);
        }

        public async Task<IEnumerable<TripTask>> SearchByTermAsync(string term)
        {
            // Arama terimini küçük harfe çevirip sorguluyoruz
            var lowerTerm = term.ToLowerInvariant();

            return await _context.TripTasks
                .Where(t => !t.IsDeleted &&
                            (t.Title.ToLower().Contains(lowerTerm) ||
                             t.Description.ToLower().Contains(lowerTerm) ||
                             t.Vehicle.PlateNumber.ToLower().Contains(lowerTerm))) // Örn: Plakayı da dahil et
                .Include(t => t.Vehicle)
                .Include(t => t.Driver)
                    .ThenInclude(d => d.User)
                .Include(t => t.Line)
                .Include(t => t.Route)
                .Include(t => t.Garage)
                .OrderBy(t => t.CreatedAt)
                .ToListAsync();
        }

        // GÖREV ÇAKIŞMA KONTROLÜ (Concurrency Logic)
        public async Task<IEnumerable<TripTask>> GetConflictingTasksAsync(
             Guid? driverId,
             Guid? vehicleId,
             DateTime startTime,
             DateTime endTime,
             Guid? currentTaskId = null)
        {
            var query = _context.TripTasks.AsQueryable();

            // 1. Temel Filtreler: Silinmişleri ve (Edit yapılıyorsa) kendisini hariç tut
            query = query.Where(t => !t.IsDeleted);

            if (currentTaskId.HasValue)
            {
                query = query.Where(t => t.Id != currentTaskId.Value);
            }

            // 2. KAYNAK KONTROLÜ (DÜZELTME BURADA)
            // Mantık: (DriverID eşleşiyor) VEYA (VehicleID eşleşiyor)
            // Eski kodda "else if" olduğu için araç kontrolü atlanıyordu.
            // Parantez içine alarak ikisinden birinin dolu olması durumunu yakalıyoruz.
            query = query.Where(t =>
                (driverId.HasValue && t.DriverId == driverId.Value) ||
                (vehicleId.HasValue && t.VehicleId == vehicleId.Value)
            );

            // 3. ZAMAN ÇAKIŞMA MANTIĞI (Overlap Logic)
            // Yeni Başlangıç < Eski Bitiş VE Yeni Bitiş > Eski Başlangıç
            query = query.Where(t =>
                t.ScheduledArrival.HasValue && t.ScheduledDeparture.HasValue && // Null check (Güvenlik için)
                startTime < t.ScheduledArrival.Value &&
                endTime > t.ScheduledDeparture.Value
            );

            // 4. Detayları getir (Hata mesajında kimin çakıştığını görmek için)
            return await query
                .Include(t => t.Driver)
                    .ThenInclude(d => d.User)
                .Include(t => t.Vehicle)
                .ToListAsync();
        }
    }
}
