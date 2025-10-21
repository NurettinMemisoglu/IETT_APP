using IETT_APP.Domain.Entities;

namespace IETT_APP.Application.Interfaces
{
    public interface IRouteRepository<T>
    {
        Task<List<Route<T>>> GetAllAsync();
        Task<Route<T>?> GetByIdAsync(T id);
        Task AddAsync(Route<T> entity);
        Task UpdateAsync(Route<T> entity);
        Task DeleteAsync(T id); // Soft delete
    }
}

