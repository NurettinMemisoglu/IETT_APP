using IETT_APP.Application.Dtos.Route;

namespace IETT_APP.Application.Interfaces
{
    public interface IRouteService<T>
    {
        Task<List<RouteDto<T>>> GetAllAsync();
        Task<RouteDto<T>?> GetByIdAsync(T id);
        Task<RouteDto<T>> CreateAsync(RouteCreateUpdateDto<T> dto);
        Task<bool> UpdateAsync(RouteCreateUpdateDto<T> dto);
        Task<bool> DeleteAsync(T id);
        Task<List<RouteDto<T>>> SearchAsync(string query);
    }
}

