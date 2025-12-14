using IETT_APP.Application.Dtos.TripTask;
using IETT_APP.Application.Wrappers;
using IETT_APP.Domain.Enums;
using IETT_APP.WebMVC.Services.Infrastructure; // BaseApiService
using IETT_APP.WebMVC.Services.Interfaces;

namespace IETT_APP.WebMVC.Services.Implementations
{
    public class TripTaskApiService : BaseApiService, ITripTaskApiService
    {
        private readonly HttpClient _httpClient;

        public TripTaskApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // ... (Mevcut CRUD metotları AYNI kalacak - GetAll, Create vb.) ...
        public async Task<IEnumerable<TripTaskDto>> GetAllAsync()
            => await _httpClient.GetFromJsonAsync<IEnumerable<TripTaskDto>>("api/triptasks") ?? new List<TripTaskDto>();

        public async Task<TripTaskDto?> GetByIdAsync(Guid id)
        {
            var response = await _httpClient.GetAsync($"api/triptasks/{id}");
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<TripTaskDto>() : null;
        }

        public async Task<ServiceResult<TripTaskDto>> CreateAsync(TripTaskCreateDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/triptasks", dto);
            return await HandleResponse<TripTaskDto>(response);
        }

        public async Task<ServiceResult<TripTaskDto>> UpdateAsync(Guid id, TripTaskUpdateDto dto)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/triptasks/{id}", dto);
            return await HandleResponse<TripTaskDto>(response);
        }

        public async Task<ServiceResult> DeleteAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"api/triptasks/{id}");
            return await HandleResponse(response);
        }

        public async Task<IEnumerable<TripTaskDto>> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return new List<TripTaskDto>();
            var encodedQuery = System.Net.WebUtility.UrlEncode(query);
            return await _httpClient.GetFromJsonAsync<IEnumerable<TripTaskDto>>($"api/triptasks/search?query={encodedQuery}") ?? new List<TripTaskDto>();
        }

        // ============================================================
        // SÜRÜCÜ OPERASYON METOTLARI (YENİ EKLENENLER)
        // ============================================================

        public async Task<IEnumerable<TripTaskDto>> GetMyTasksAsync()
        {
            var response = await _httpClient.GetAsync("api/triptasks/my-tasks");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<IEnumerable<TripTaskDto>>() ?? new List<TripTaskDto>();
            }
            return new List<TripTaskDto>();
        }

        public async Task<ServiceResult> AcceptTripAsync(Guid id)
        {
            var response = await _httpClient.PatchAsync($"api/triptasks/{id}/accept", null);
            return await HandleResponse(response);
        }

        public async Task<ServiceResult> RejectTripAsync(Guid id, RejectTripRequestDto dto)
        {
            var response = await _httpClient.PatchAsJsonAsync($"api/triptasks/{id}/reject", dto);
            return await HandleResponse(response);
        }

        public async Task<ServiceResult> StartTripAsync(Guid id)
        {
            var response = await _httpClient.PatchAsync($"api/triptasks/{id}/start", null);
            return await HandleResponse(response);
        }

        public async Task<ServiceResult> CompleteTripAsync(Guid id, CompleteTripRequestDto dto)
        {
            var response = await _httpClient.PatchAsJsonAsync($"api/triptasks/{id}/complete", dto);
            return await HandleResponse(response);
        }

        public async Task<ServiceResult> FailTripAsync(Guid id, FailTripRequestDto dto)
        {
            var response = await _httpClient.PatchAsJsonAsync($"api/triptasks/{id}/fail", dto);
            return await HandleResponse(response);
        }
        public List<TaskState> GetAllowedStatesForRole(string role)
        {
            if (role == "Chief" || role == "Admin") return new List<TaskState> { TaskState.Pending, TaskState.Cancelled };
            if (role == "Driver") return new List<TaskState> { TaskState.Accepted, TaskState.InProgress, TaskState.Completed, TaskState.Incomplete };
            return new List<TaskState>();
        }
    }
}