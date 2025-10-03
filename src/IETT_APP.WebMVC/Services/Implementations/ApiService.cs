using IETT_APP.Applicaton.Dtos;
using MVCProject.Services.Interfaces;
using MVCProject.ViewModels;
using System.Text;
using System.Text.Json;

namespace IETT_APP.WebMVC.Services.Implementations
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public void SetTokenHeader(string token)
        {
            if (_httpClient.DefaultRequestHeaders.Contains("Authorization"))
                _httpClient.DefaultRequestHeaders.Remove("Authorization");

            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
        }

        public void RemoveTokenHeader()
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        public string GetToken()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString("JwtToken") ?? string.Empty;
        }

        public async Task<(bool IsSuccess, string Message)> RegisterAsync(RegisterViewModel model)
        {
            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/auth/register", content);

            if (response.IsSuccessStatusCode)
                return (true, "Kayıt başarılı!");

            var error = await response.Content.ReadAsStringAsync();
            return (false, error);
        }

        public async Task<(bool IsSuccess, string Message, string Token)> LoginAsync(LoginViewModel model)
        {
            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/auth/login", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return (false, error, string.Empty);
            }

            var responseObj = await JsonSerializer.DeserializeAsync<AuthResponseDto>(
                await response.Content.ReadAsStreamAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (responseObj == null || string.IsNullOrEmpty(responseObj.AccessToken))
            {
                return (false, "Token alınamadı", string.Empty);
            }

            // Save tokens in session and set header
            _httpContextAccessor.HttpContext?.Session.SetString("JwtToken", responseObj.AccessToken);
            if (!string.IsNullOrEmpty(responseObj.RefreshToken))
            {
                _httpContextAccessor.HttpContext?.Session.SetString("RefreshToken", responseObj.RefreshToken);
            }
            SetTokenHeader(responseObj.AccessToken);

            return (true, "Giriş başarılı!", responseObj.AccessToken);
        }

        public class LoginResponseDto
        {
            public string Token { get; set; }
        }
    }
}
