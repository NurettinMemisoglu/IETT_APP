using IETT_APP.Application.Dtos.TripTask;
using IETT_APP.WebMVC.Services.Interfaces;
using System.Text.Json;

namespace IETT_APP.WebMVC.Services.Implementations
{
    public class TripTaskApiService : ITripTaskApiService
    {
        private readonly HttpClient _httpClient;

        public TripTaskApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<TripTaskDto>> GetAllAsync()
            => await _httpClient.GetFromJsonAsync<IEnumerable<TripTaskDto>>("api/TripTasks")
               ?? new List<TripTaskDto>();


        public async Task<TripTaskDto?> GetByIdAsync(Guid id)
            => await _httpClient.GetFromJsonAsync<TripTaskDto>($"api/TripTasks/{id}");


        public async Task<TripTaskDto> CreateAsync(TripTaskCreateDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/TripTasks", dto);
            var content = await response.Content.ReadAsStringAsync();

            Console.WriteLine("API CREATE RESPONSE STATUS: " + response.StatusCode);
            Console.WriteLine("API CREATE RESPONSE BODY: " + content);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"API hata: {response.StatusCode} - {content}");

            return JsonSerializer.Deserialize<TripTaskDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new Exception("Oluşturma işlemi başarısız oldu.");
        }


        public async Task<TripTaskDto> UpdateAsync(Guid id, TripTaskUpdateDto dto)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/TripTasks/{id}", dto);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"API hata: {response.StatusCode} - {content}");

            return JsonSerializer.Deserialize<TripTaskDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new Exception("Güncelleme işlemi başarısız oldu.");
        }


        public async Task<bool> DeleteAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"api/TripTasks/{id}");
            Console.WriteLine($"Silme isteği: api/TripTasks/{id}");
            return response.IsSuccessStatusCode;
        }


        public async Task<IEnumerable<TripTaskDto>> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<TripTaskDto>();

            var encodedQuery = System.Net.WebUtility.UrlEncode(query);

            return await _httpClient.GetFromJsonAsync<IEnumerable<TripTaskDto>>(
                       $"api/TripTasks/search?query={encodedQuery}")
                   ?? new List<TripTaskDto>();
        }
    }
}
