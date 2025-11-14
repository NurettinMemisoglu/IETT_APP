using IETT_APP.Application.Dtos.TripTask;

namespace IETT_APP.WebMVC.Services.Interfaces
{
    public interface ITripTaskApiService
    {
        Task<IEnumerable<TripTaskDto>> GetAllAsync();
        Task<TripTaskDto?> GetByIdAsync(Guid id);
        Task<TripTaskDto> CreateAsync(TripTaskCreateDto dto);
        Task<TripTaskDto> UpdateAsync(Guid id, TripTaskUpdateDto dto);
        Task<bool> DeleteAsync(Guid id);
        Task<IEnumerable<TripTaskDto>> SearchAsync(string query);
    }
}
