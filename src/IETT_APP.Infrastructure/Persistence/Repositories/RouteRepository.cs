using IETT_APP.Application.Interfaces;
using IETT_APP.Domain.Entities;
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
                                 .Include(l => l.RouteStops)
                                 .ToListAsync();
        }

        public async Task<Route<T>?> GetByIdAsync(T id)
        {
            return await _context.Set<Route<T>>()
                                 .Include(l => l.RouteStops)
                                 .FirstOrDefaultAsync(x => x.Id!.Equals(id));
        }

        public async Task AddAsync(Route<T> entity)
        {
            var exists = await _context.Set<Route<T>>()
                           .AnyAsync(x => x.Code == entity.Code && !x.IsDeleted);

            if (exists)
                throw new ArgumentException($"Aynı güzergah kodu ile aktif bir hat mevcut: {entity.Code}");

            entity.CreatedAt = DateTime.UtcNow;
            entity.CreatedBy = "System";
            await _context.Set<Route<T>>().AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Route<T> entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = "System";
            _context.Set<Route<T>>().Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(T id)
        {
            var entity = await _context.Set<Route<T>>().FirstOrDefaultAsync(x => x.Id!.Equals(id));
            if (entity == null) return;

            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;
            entity.DeletedBy = "System";

            _context.Set<Route<T>>().Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}
