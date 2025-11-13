using IETT_APP.Domain.Entities;

namespace IETT_APP.Domain.Interfaces
{
    public interface ITripTaskRepository
    {
        Task<TripTask?> GetByIdAsync(Guid id);
        Task<IEnumerable<TripTask>> GetAllAsync();
        Task AddAsync(TripTask tripTask);
        Task UpdateAsync(TripTask tripTask);
        Task SoftDeleteAsync(TripTask tripTask, string? reason = null);
        Task<bool> ExistsAsync(Guid id);
    }
}
