using IETT_APP.Application.Dtos.Vehicle;

namespace IETT_APP.Application.Interfaces
{

    public interface IVehicleService<T>
    {
        Task<IEnumerable<VehicleDto<T>>> GetAllAsync();
        Task<VehicleDto<T>?> GetByIdAsync(T id);
        Task<T> AddAsync(VehicleCreateDto<T> dto);
        Task UpdateAsync(VehicleUpdateDto<T> dto);
        Task<bool> DeleteAsync(T id);
        Task<List<VehicleDto<T>>> SearchAsync(string query);
        Task UnassignFromLineAsync(T vehicleId);
        Task<List<VehicleDto<T>>> GetUnassignedVehiclesAsync(); // Hat atanmış olmayan araçlar
    }


}
