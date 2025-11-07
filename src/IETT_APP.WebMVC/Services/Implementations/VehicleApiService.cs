using IETT_APP.Application.Dtos.Vehicle;
using IETT_APP.WebMVC.Services.Interfaces;
using System.Text.Json;

namespace IETT_APP.WebMVC.Services.Implementations
{
    public class VehicleApiService : IVehicleApiService
    {
        private readonly HttpClient _httpClient;

        public VehicleApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<VehicleDto<Guid>>> GetAllAsync()
            => await _httpClient.GetFromJsonAsync<IEnumerable<VehicleDto<Guid>>>("api/vehicles") ?? new List<VehicleDto<Guid>>();

        public async Task<VehicleDto<Guid>?> GetByIdAsync(Guid id)
            => await _httpClient.GetFromJsonAsync<VehicleDto<Guid>>($"api/vehicles/{id}");

        public async Task<VehicleDto<Guid>> CreateAsync(VehicleCreateDto<Guid> dto)
        {
            var json = JsonSerializer.Serialize(dto);
            Console.WriteLine("GÖNDERİLEN JSON: " + json);

            var response = await _httpClient.PostAsJsonAsync("api/vehicles/create", dto);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"API hata: {response.StatusCode} - {content}");

            return JsonSerializer.Deserialize<VehicleDto<Guid>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new Exception("İşlem başarısız oldu.");
        }

        public async Task<VehicleDto<Guid>> UpdateAsync(Guid id, VehicleUpdateDto<Guid> dto)
        {
            var json = JsonSerializer.Serialize(dto);
            Console.WriteLine("GÖNDERİLEN JSON (UPDATE): " + json);

            var response = await _httpClient.PutAsJsonAsync($"api/vehicles/update/{id}", dto);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"API hata: {response.StatusCode} - {content}");

            return JsonSerializer.Deserialize<VehicleDto<Guid>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new Exception("Güncelleme başarısız oldu.");
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"api/vehicles/{id}");
            Console.WriteLine($"Silme isteği: api/vehicles/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<IEnumerable<VehicleDto<Guid>>> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return new List<VehicleDto<Guid>>();
            var encodedQuery = System.Net.WebUtility.UrlEncode(query);
            return await _httpClient.GetFromJsonAsync<IEnumerable<VehicleDto<Guid>>>($"api/vehicles/search?query={encodedQuery}")
                   ?? new List<VehicleDto<Guid>>();
        }
    }
}
