using IETT_APP.Domain.Entities;
using IETT_APP.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IETT_APP.Infrastructure.Persistence.Repositories
{
    public class GarageRepository : IGarageRepository
    {
        private readonly AppDbContext _context;

        public GarageRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Garage<Guid>>> GetAllAsync()
        {
            return await _context.Garages
                .Where(g => !g.IsDeleted)
                .ToListAsync();
        }

        public async Task<Garage<Guid>?> GetByIdAsync(Guid id)
        {
            return await _context.Garages
                .FirstOrDefaultAsync(g => g.Id == id && !g.IsDeleted);
        }

        // Garage içinde araç var mı kontrolü
        public async Task<bool> IsVehicleInGarageAsync(Guid garageId, Guid vehicleId)
        {
            return await _context.Vehicles
                .AnyAsync(v => v.Id == vehicleId && v.GarageId == garageId && !v.IsDeleted);
        }
    }
}
