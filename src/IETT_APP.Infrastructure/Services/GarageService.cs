using IETT_APP.Application.Dtos.Garage;
using IETT_APP.Application.Interfaces.Garages;
using IETT_APP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IETT_APP.Infrastructure.Services
{
    public class GarageService : IGarageService
    {
        private readonly AppDbContext _context;

        public GarageService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<GarageDto<Guid>>> GetAllAsync()
        {
            return await _context.Garages
                .Select(g => new GarageDto<Guid>
                {
                    Id = g.Id,
                    GarageName = g.GarageName
                })
                .ToListAsync();
        }

        public async Task<GarageDto<Guid>?> GetByIdAsync(Guid id)
        {
            var entity = await _context.Garages
                .Where(g => g.Id == id)
                .Select(g => new GarageDto<Guid>
                {
                    Id = g.Id,
                    GarageName = g.GarageName
                })
                .FirstOrDefaultAsync();

            return entity;
        }
    }
}
