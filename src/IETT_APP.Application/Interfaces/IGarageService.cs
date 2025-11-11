using IETT_APP.Application.Dtos.Garage;

namespace IETT_APP.Application.Interfaces.Garages
{
    public interface IGarageService
    {
        Task<IEnumerable<GarageDto<Guid>>> GetAllAsync();
        Task<GarageDto<Guid>?> GetByIdAsync(Guid id);
    }
}