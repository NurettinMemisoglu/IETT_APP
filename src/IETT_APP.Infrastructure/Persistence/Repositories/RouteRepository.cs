using IETT_APP.Domain.Entities;
using IETT_APP.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IETT_APP.Infrastructure.Persistence.Repositories
{
    public class RouteRepository<T> : IRouteRepository<T>
    {
        private readonly AppDbContext _context;

        public RouteRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Route<T>>> GetAllAsync()
        {
            return await _context.Set<Route<T>>()
                .Include(r => r.RouteStops)
                .ThenInclude(rs => rs.Stop)
                .Where(r => !r.IsDeleted) // <-- sadece aktif route’lar
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();
        }


        public async Task<Route<T>?> GetByIdAsync(T id)
        {
            return await _context.Set<Route<T>>()
                .Include(r => r.RouteStops)
                .ThenInclude(rs => rs.Stop)
                .FirstOrDefaultAsync(r => r.Id!.Equals(id) && !r.IsDeleted); // <-- filtre eklendi
        }


        public async Task AddAsync(Route<T> entity)
        {
            _context.Set<Route<T>>().Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Route<T> entity)
        {
            _context.Set<Route<T>>().Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(T id)
        {
            var entity = await _context.Set<Route<T>>().FirstOrDefaultAsync(x => x.Id!.Equals(id));
            if (entity == null) return;

            entity.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }
}
