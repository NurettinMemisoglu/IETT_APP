using IETT_APP.Application.Dtos;
using IETT_APP.Application.Wrappers;
using IETT_APP.WebMVC.Services.Infrastructure; // BaseApiService
using IETT_APP.WebMVC.Services.Interfaces;

namespace IETT_APP.WebMVC.Services.Implementations
{
    // BaseApiService'den miras alarak ortak metotlara erişiyoruz
    public class AuthApiService : BaseApiService, IAuthApiService
    {
        private readonly HttpClient _httpClient;

        public AuthApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // --- LOGIN ---
        public async Task<AuthResponseDto?> LoginAsync(LoginUserDto loginDto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginDto);

            if (response.IsSuccessStatusCode)
            {
                // Helper metoda gerek yok, standart extension metot yeterli
                return await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            }

            return null;
        }

        // --- REGISTER ---
        public async Task<AuthResponseDto?> RegisterAsync(RegisterUserDto registerDto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", registerDto);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            }

            return null;
        }

        // --- CHANGE PASSWORD ---
        public async Task<ServiceResult> ChangePasswordAsync(ChangePasswordDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/change-password", dto);

            // BaseApiService içindeki HandleResponse'u kullanıyoruz (Kod tekrarı bitti)
            return await HandleResponse(response);
        }

        // --- LOGOUT ---
        public async Task LogoutAsync()
        {
            await Task.CompletedTask;
        }
    }
}