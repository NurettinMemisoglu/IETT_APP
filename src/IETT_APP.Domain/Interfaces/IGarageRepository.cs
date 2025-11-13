using IETT_APP.Domain.Entities;

namespace IETT_APP.Domain.Interfaces
{
    public interface IGarageRepository
    {
        Task<IEnumerable<Garage<Guid>>> GetAllAsync();
        Task<Garage<Guid>?> GetByIdAsync(Guid id);
        Task<bool> IsVehicleInGarageAsync(Guid garageId, Guid vehicleId);
    }

}
