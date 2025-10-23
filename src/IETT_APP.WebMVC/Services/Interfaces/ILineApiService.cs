using IETT_APP.Application.Dtos.Line;

namespace IETT_APP.WebMVC.Services.Interfaces
{
    public interface ILineApiService
    {
        Task<IEnumerable<LineDto<Guid>>> GetAllAsync();
        Task<LineDto<Guid>?> GetByIdAsync(Guid id);
        Task<LineDto<Guid>> CreateOrUpdateAsync(LineCreateUpdateDto<Guid> dto); // Tek metod
        Task<bool> DeleteAsync(Guid id);
        Task<IEnumerable<LineDto<Guid>>> SearchAsync(string query);
        Task<bool> SetActiveAsync(Guid id, bool isActive);
    }
}
