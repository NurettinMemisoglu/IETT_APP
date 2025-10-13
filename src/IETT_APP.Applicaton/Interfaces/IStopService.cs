using IETT_APP.Application.Dtos;
using IETT_APP.Applicaton.Dtos.Stop;

namespace IETT_APP.Application.Interfaces
{
    public interface IStopService
    {
        Task<List<StopDto>> GetAllAsync();
        Task<StopDto?> GetByIdAsync(string id);
        Task<StopDto> CreateAsync(CreateStopDto dto);
        Task<bool> UpdateAsync(string id, UpdateStopDto dto);
        Task<bool> DeleteAsync(string id);
        Task<List<StopDto>> SearchByNameAsync(string name);
    }
}
