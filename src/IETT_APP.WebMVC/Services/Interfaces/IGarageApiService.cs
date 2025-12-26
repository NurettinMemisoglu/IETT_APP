using IETT_APP.Application.Dtos.Garage;
using IETT_APP.WebMVC.Areas.Planner.Models;

namespace IETT_APP.WebMVC.Services.Interfaces
{
    public interface IGarageApiService
    {
        Task<IEnumerable<GarageViewModel>> GetAllAsync();
        Task<GarageDto?> GetByIdAsync(Guid id);
    }
}
