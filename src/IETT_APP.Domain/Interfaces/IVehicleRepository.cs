using IETT_APP.Domain.Entities;

namespace IETT_APP.Domain.Interfaces
{
    public interface IVehicleRepository<T>
    {
        Task<IEnumerable<Vehicle<T>>> GetAllAsync();
        Task<Vehicle<T>?> GetByIdAsync(T id);
        Task AddAsync(Vehicle<T> vehicle);
        Task UpdateAsync(Vehicle<T> vehicle);
        Task DeleteAsync(T id);

    }
}
