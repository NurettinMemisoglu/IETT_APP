using IETT_APP.Application.Dtos.Route;

namespace IETT_APP.WebMVC.Services.Interfaces
{
    public interface IRouteApiService
    {
        Task<IEnumerable<RouteDto<Guid>>> GetAllAsync();
        Task<RouteDto<Guid>?> GetByIdAsync(Guid id);
        Task<RouteDto<Guid>> CreateOrUpdateAsync(RouteCreateUpdateDto<Guid> dto); // Tek metod
        Task<bool> DeleteAsync(Guid id);
        Task<IEnumerable<RouteDto<Guid>>> SearchAsync(string query);
        Task<bool> SetActiveAsync(Guid id, bool isActive);
    }
}
