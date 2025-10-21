using IETT_APP.Application.Dtos.Stop;

namespace IETT_APP.Application.Interfaces
{
    public interface IStopService
    {
        Task<List<StopDto>> GetAllAsync();
        Task<StopDto?> GetByIdAsync(Guid id);               // string → Guid
        Task<StopDto> CreateAsync(CreateStopDto dto);
        Task<bool> UpdateAsync(Guid id, UpdateStopDto dto); // string → Guid
        Task<bool> DeleteAsync(Guid id);                   // string → Guid
        Task<List<StopDto>> SearchByNameAsync(string name);
    }

}
