using IETT_APP.Application.Interfaces;
using IETT_APP.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IETT_APP.Infrastructure.Persistence.Repositories
{
    public class LineRepository<T> : ILineRepository<T>
    {
        private readonly AppDbContext _context;
        private static readonly TimeZoneInfo TurkeyZone =
            TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");

        public LineRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Line<T>>> GetAllAsync()
        {
            return await _context.Set<Line<T>>()
                            .OrderBy(v => v.CreatedAt)
                            .ToListAsync();
        }

        public async Task<Line<T>?> GetByIdAsync(T id)
        {
            return await _context.Set<Line<T>>()
                                 .FirstOrDefaultAsync(x => x.Id!.Equals(id));
        }

        public async Task AddAsync(Line<T> entity)
        {
            var exists = await _context.Set<Line<T>>()
                .AnyAsync(x => x.Code == entity.Code && !x.IsDeleted);

            if (exists)
                throw new ArgumentException($"Aynı hat kodu ile aktif bir hat mevcut: {entity.Code}");

            var nowTr = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TurkeyZone);

            entity.CreatedAt = nowTr;
            entity.CreatedBy = "System";

            await _context.Set<Line<T>>().AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Line<T> entity)
        {
            var nowTr = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TurkeyZone);

            entity.UpdatedAt = nowTr;
            entity.UpdatedBy = "System";

            _context.Set<Line<T>>().Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(T id)
        {
            var entity = await _context.Set<Line<T>>().FirstOrDefaultAsync(x => x.Id!.Equals(id));
            if (entity == null) return;

            var nowTr = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TurkeyZone);

            entity.IsDeleted = true;
            entity.DeletedAt = nowTr;
            entity.DeletedBy = "System";

            _context.Set<Line<T>>().Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}
