using IETT_APP.Application.Dtos.Line;

namespace IETT_APP.Application.Interfaces
{
    public interface ILineService<T>
    {
        Task<List<LineDto<T>>> GetAllAsync();
        Task<LineDto<T>?> GetByIdAsync(T id);
        Task<LineDto<T>> CreateAsync(LineCreateUpdateDto<T> dto);
        Task<bool> UpdateAsync(LineCreateUpdateDto<T> dto);
        Task<bool> DeleteAsync(T id);
        Task<List<LineDto<T>>> SearchAsync(string query);
        Task<bool> SetActiveAsync(T id, bool isActive);

        // ✅ Yeni metodlar
        Task<LineDto<T>> CreateOrUpdateAsync(LineCreateUpdateDto<T> dto);
        Task<bool> SoftDeleteAsync(T id);
    }
}
