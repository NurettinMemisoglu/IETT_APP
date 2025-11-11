using IETT_APP.Application.Dtos.Garage;
using IETT_APP.WebMVC.Areas.Planner.Models;
using IETT_APP.WebMVC.Services.Interfaces;

namespace IETT_APP.WebMVC.Services.Implementations
{
    public class GarageApiService : IGarageApiService
    {
        private readonly HttpClient _httpClient;

        public GarageApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // 🔹 Tüm garajları getir
        public async Task<IEnumerable<GarageViewModel>> GetAllAsync()
        {
            try
            {
                var garages = await _httpClient.GetFromJsonAsync<IEnumerable<GarageViewModel>>("api/garages");
                return garages ?? Enumerable.Empty<GarageViewModel>();
            }
            catch
            {
                // Hata durumunda boş liste döndür
                return Enumerable.Empty<GarageViewModel>();
            }
        }
        public async Task<GarageDto<Guid>?> GetByIdAsync(Guid id)
            => await _httpClient.GetFromJsonAsync<GarageDto<Guid>>($"api/garages/{id}");
    }
}
