using IETT_APP.Application.Dtos.Vehicle;

namespace IETT_APP.WebMVC.Services.Interfaces
{
    public interface IVehicleApiService
    {
        Task<IEnumerable<VehicleDto<Guid>>> GetAllAsync();
        Task<VehicleDto<Guid>?> GetByIdAsync(Guid id);
        Task<VehicleDto<Guid>> CreateAsync(VehicleCreateDto<Guid> dto);
        Task<VehicleDto<Guid>> UpdateAsync(Guid id, VehicleUpdateDto<Guid> dto);
        Task<bool> DeleteAsync(Guid id);
        Task<IEnumerable<VehicleDto<Guid>>> SearchAsync(string query);
    }
}
