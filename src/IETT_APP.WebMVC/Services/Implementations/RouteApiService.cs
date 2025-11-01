using IETT_APP.Application.Dtos.Route;
using IETT_APP.WebMVC.Services.Interfaces;
using System.Text.Json;

namespace IETT_APP.WebMVC.Services.Implementations
{
    public class RouteApiService : IRouteApiService
    {
        private readonly HttpClient _httpClient;

        public RouteApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<RouteDto<Guid>>> GetAllAsync()
            => await _httpClient.GetFromJsonAsync<IEnumerable<RouteDto<Guid>>>("api/routes") ?? new List<RouteDto<Guid>>();

        public async Task<RouteDto<Guid>?> GetByIdAsync(Guid id)
            => await _httpClient.GetFromJsonAsync<RouteDto<Guid>>($"api/routes/{id}");

        public async Task<RouteDto<Guid>> CreateOrUpdateAsync(RouteCreateUpdateDto<Guid> dto)
        {
            var json = JsonSerializer.Serialize(dto);
            Console.WriteLine("GÖNDERİLEN JSON: " + json);

            var response = await _httpClient.PostAsJsonAsync("api/routes/execute", dto);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"API hata: {response.StatusCode} - {content}");

            return JsonSerializer.Deserialize<RouteDto<Guid>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new Exception("İşlem başarısız oldu.");
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"api/routes/{id}");
            Console.WriteLine($"Silme isteği: api/routes/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<IEnumerable<RouteDto<Guid>>> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return new List<RouteDto<Guid>>();
            var encodedQuery = System.Net.WebUtility.UrlEncode(query);
            return await _httpClient.GetFromJsonAsync<IEnumerable<RouteDto<Guid>>>($"api/routes/search?query={encodedQuery}")
                   ?? new List<RouteDto<Guid>>();
        }

        public async Task<bool> SetActiveAsync(Guid id, bool isActive)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/routes/{id}/activate-deactivate", new { isActive });
            return response.IsSuccessStatusCode;
        }
    }
}
