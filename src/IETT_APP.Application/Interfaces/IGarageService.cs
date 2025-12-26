using IETT_APP.Application.Dtos.Garage;

namespace IETT_APP.Application.Interfaces.Garages
{
    public interface IGarageService
    {
        Task<IEnumerable<GarageDto>> GetAllAsync();
        Task<GarageDto?> GetByIdAsync(Guid id);
    }
}