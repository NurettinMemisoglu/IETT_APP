using IETT_APP.Application.Dtos.Stop;
using IETT_APP.WebMVC.Services.Interfaces;
using System.Text.Json;

namespace IETT_APP.WebMVC.Services.Implementations
{
    public class StopService : IStopService
    {
        private readonly HttpClient _http;

        public StopService(HttpClient http)
        {
            _http = http;
        }

        public async Task<IEnumerable<StopDto>> GetAllAsync()
        {
            var result = await _http.GetFromJsonAsync<IEnumerable<StopDto>>("api/stops");
            return result ?? new List<StopDto>();
        }

        public async Task<StopDto?> GetByIdAsync(string id)
        {
            return await _http.GetFromJsonAsync<StopDto>($"api/stops/{id}");
        }

        public async Task<StopDto> CreateAsync(CreateStopDto dto)
        {
            var response = await _http.PostAsJsonAsync("api/stops", dto);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"API hata: {response.StatusCode} - {content}");

            return JsonSerializer.Deserialize<StopDto>(content)
                   ?? throw new Exception("Stop creation failed");

        }

        public async Task<bool> UpdateAsync(string id, UpdateStopDto dto)
        {
            var response = await _http.PutAsJsonAsync($"api/stops/{id}", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var response = await _http.DeleteAsync($"api/stops/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<IEnumerable<StopDto>> SearchByNameAsync(string name)
        {
            var result = await _http.GetFromJsonAsync<IEnumerable<StopDto>>($"api/stops/search?name={name}");
            return result ?? new List<StopDto>();
        }
    }
}
