using IETT_APP.Application.Dtos.Stop;

namespace IETT_APP.WebMVC.Services.Interfaces
{
    public interface IStopApiService
    {
        Task<IEnumerable<StopDto>> GetAllAsync();
        Task<StopDto?> GetByIdAsync(string id);
        Task<StopDto> CreateAsync(CreateStopDto dto);
        Task<bool> UpdateAsync(string id, UpdateStopDto dto);
        Task<bool> DeleteAsync(string id);
        Task<IEnumerable<StopDto>> SearchByNameAsync(string name);
    }
}
