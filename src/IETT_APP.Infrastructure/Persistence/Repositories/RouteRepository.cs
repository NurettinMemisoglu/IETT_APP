using IETT_APP.Application.Interfaces;
using IETT_APP.Domain.Entities;
using IETT_APP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class RouteRepository<T> : IRouteRepository<T>
{
    private readonly AppDbContext _context;
    private static readonly TimeZoneInfo TurkeyZone =
        TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");

    public RouteRepository(AppDbContext context)
    {
        _context = context;
    }

    // 🔹 GetAllAsync: RouteStops ve Stop bilgilerini yükle
    public async Task<List<Route<T>>> GetAllAsync()
    {
        return await _context.Set<Route<T>>()
                             .Include(l => l.RouteStops)
                                 .ThenInclude(rs => rs.Stop)
                             .ToListAsync();
    }

    // 🔹 GetByIdAsync: tek bir Route için Stop bilgilerini yükle
    public async Task<Route<T>?> GetByIdAsync(T id)
    {
        return await _context.Set<Route<T>>()
                             .Include(l => l.RouteStops)
                                 .ThenInclude(rs => rs.Stop)
                             .FirstOrDefaultAsync(x => x.Id!.Equals(id));
    }

    public async Task AddAsync(Route<T> entity)
    {
        var exists = await _context.Set<Route<T>>()
                       .AnyAsync(x => x.Code == entity.Code && !x.IsDeleted);

        if (exists)
            throw new ArgumentException($"Aynı güzergah kodu ile aktif bir hat mevcut: {entity.Code}");

        // 🇹🇷 TR saatini kullan
        var nowTr = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TurkeyZone);

        entity.CreatedAt = nowTr;
        entity.CreatedBy = "System";

        await _context.Set<Route<T>>().AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Route<T> entity)
    {
        var nowTr = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TurkeyZone);

        entity.UpdatedAt = nowTr;
        entity.UpdatedBy = "System";

        _context.Set<Route<T>>().Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(T id)
    {
        var entity = await _context.Set<Route<T>>().FirstOrDefaultAsync(x => x.Id!.Equals(id));
        if (entity == null) return;

        var nowTr = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TurkeyZone);

        entity.IsDeleted = true;
        entity.DeletedAt = nowTr;
        entity.DeletedBy = "System";

        _context.Set<Route<T>>().Update(entity);
        await _context.SaveChangesAsync();
    }
}
