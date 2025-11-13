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

        public async Task<IEnumerable<TripTask>> GetAllAsync()
        {
            return await _context.TripTasks
                .Where(t => !t.IsDeleted)
                .Include(t => t.Vehicle)
                .Include(t => t.Operator)
                .Include(t => t.Line)
                .Include(t => t.Route)
                .Include(t => t.Garage)
                .ToListAsync();
        }

        public async Task<TripTask?> GetByIdAsync(Guid id)
        {
            return await _context.TripTasks
                .Include(t => t.TripTaskHistories)
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
        }

        public async Task UpdateAsync(TripTask tripTask)
        {
            _context.TripTasks.Update(tripTask);
            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteAsync(TripTask tripTask, string? reason = null)
        {
            if (!tripTask.IsDeleted)
            {
                tripTask.IsDeleted = true;
                tripTask.UpdatedAt = DateTime.UtcNow;

                tripTask.TripTaskHistories.Add(new TripTaskHistory
                {
                    TripTaskId = tripTask.Id,
                    OldValue = null,
                    NewValue = $"Soft deleted. Reason: {reason}",
                    CreatedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.TripTasks.AnyAsync(t => t.Id == id && !t.IsDeleted);
        }
    }
}
