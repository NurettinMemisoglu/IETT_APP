using IETT_APP.Application.Dtos;
using IETT_APP.WebMVC.Services.Interfaces;
using MVCProject.ViewModels;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MVCProject.Services
{
    public class ProfileService : IProfileService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProfileService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        private void SetBearerToken()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<(bool IsSuccess, string Message, ProfileViewModel? Data)> GetProfileAsync()
        {
            SetBearerToken();

            var response = await _httpClient.GetAsync("api/profile/me");
            if (response.StatusCode == HttpStatusCode.Unauthorized && await TryRefreshAsync())
            {
                SetBearerToken();
                response = await _httpClient.GetAsync("api/profile/me");
            }

            if (!response.IsSuccessStatusCode)
                return (false, "Profil bilgisi alınamadı.", null);

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<ProfileViewModel>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return (true, "Profil bilgisi başarıyla alındı.", data);
        }

        public async Task<(bool IsSuccess, string Message)> UpdateProfileAsync(ProfileViewModel model)
        {
            SetBearerToken();

            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync("api/profile/update", content);
            if (response.StatusCode == HttpStatusCode.Unauthorized && await TryRefreshAsync())
            {
                SetBearerToken();
                response = await _httpClient.PutAsync("api/profile/update", content);
            }

            if (!response.IsSuccessStatusCode)
                return (false, "Profil güncellenemedi.");

            return (true, "Profil başarıyla güncellendi.");
        }

        public async Task<(bool IsSuccess, string Message)> ChangePasswordAsync(ChangePasswordViewModel model)
        {
            SetBearerToken();

            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/profile/change-password", content);
            if (response.StatusCode == HttpStatusCode.Unauthorized && await TryRefreshAsync())
            {
                SetBearerToken();
                response = await _httpClient.PostAsync("api/profile/change-password", content);
            }

            if (!response.IsSuccessStatusCode)
                return (false, "Şifre değiştirilemedi.");

            return (true, "Şifre başarıyla değiştirildi.");
        }

        private async Task<bool> TryRefreshAsync()
        {
            var refresh = _httpContextAccessor.HttpContext?.Session.GetString("RefreshToken");
            if (string.IsNullOrEmpty(refresh)) return false;

            var res = await _httpClient.PostAsJsonAsync("api/auth/refresh", refresh);
            if (!res.IsSuccessStatusCode) return false;

            var dto = await res.Content.ReadFromJsonAsync<AuthResponseDto>();
            if (dto == null || string.IsNullOrEmpty(dto.AccessToken)) return false;

            _httpContextAccessor.HttpContext?.Session.SetString("JwtToken", dto.AccessToken);
            if (!string.IsNullOrEmpty(dto.RefreshToken))
            {
                _httpContextAccessor.HttpContext?.Session.SetString("RefreshToken", dto.RefreshToken);
            }
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", dto.AccessToken);
            return true;
        }
    }
}
