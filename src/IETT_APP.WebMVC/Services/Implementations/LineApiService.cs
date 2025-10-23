using IETT_APP.Application.Dtos.Line;
using IETT_APP.WebMVC.Services.Interfaces;
using System.Text.Json;

namespace IETT_APP.WebMVC.Services.Implementations
{
    public class LineApiService : ILineApiService
    {
        private readonly HttpClient _httpClient;

        public LineApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<LineDto<Guid>>> GetAllAsync()
            => await _httpClient.GetFromJsonAsync<IEnumerable<LineDto<Guid>>>("api/lines") ?? new List<LineDto<Guid>>();

        public async Task<LineDto<Guid>?> GetByIdAsync(Guid id)
            => await _httpClient.GetFromJsonAsync<LineDto<Guid>>($"api/lines/{id}");

        public async Task<LineDto<Guid>> CreateOrUpdateAsync(LineCreateUpdateDto<Guid> dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/lines/execute", dto);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"API hata: {response.StatusCode} - {content}");

            return JsonSerializer.Deserialize<LineDto<Guid>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new Exception("İşlem başarısız oldu.");
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"api/lines/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<IEnumerable<LineDto<Guid>>> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return new List<LineDto<Guid>>();
            var encodedQuery = System.Net.WebUtility.UrlEncode(query);
            return await _httpClient.GetFromJsonAsync<IEnumerable<LineDto<Guid>>>($"api/lines/search?query={encodedQuery}")
                   ?? new List<LineDto<Guid>>();
        }

        public async Task<bool> SetActiveAsync(Guid id, bool isActive)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/lines/{id}/set-active", new { isActive });
            return response.IsSuccessStatusCode;
        }
    }
}
