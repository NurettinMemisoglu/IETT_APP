using IETT_APP.Application.Interfaces;
using IETT_APP.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IETT_APP.Infrastructure.Persistence.Repositories
{
    public class LineRepository<T> : ILineRepository<T>
    {
        private readonly AppDbContext _context;

        public LineRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Line<T>>> GetAllAsync()
        {
            // No invalid Include here — return lines only.
            return await _context.Set<Line<T>>().ToListAsync();
        }

        public async Task<Line<T>?> GetByIdAsync(T id)
        {
            return await _context.Set<Line<T>>()
                                 .FirstOrDefaultAsync(x => x.Id!.Equals(id));
        }

        public async Task AddAsync(Line<T> entity)
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.CreatedBy = "System";
            await _context.Set<Line<T>>().AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Line<T> entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = "System";
            _context.Set<Line<T>>().Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(T id)
        {
            var entity = await _context.Set<Line<T>>().FirstOrDefaultAsync(x => x.Id!.Equals(id));
            if (entity == null) return;

            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;
            entity.DeletedBy = "System";

            _context.Set<Line<T>>().Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}
