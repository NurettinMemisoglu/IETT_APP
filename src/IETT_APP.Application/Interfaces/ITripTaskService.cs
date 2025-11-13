using IETT_APP.Application.Dtos.TripTask;

namespace IETT_APP.Application.Interfaces
{
    public interface ITripTaskService
    {
        Task<TripTaskDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<TripTaskDto>> GetAllAsync();
        Task<Guid> AddAsync(TripTaskCreateDto dto);
        Task UpdateAsync(TripTaskUpdateDto dto);
        Task DeleteAsync(Guid id, string? reason = null);
    }
}
