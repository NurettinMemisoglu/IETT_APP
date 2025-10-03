using IETT_APP.Applicaton.Dtos;
using IETT_APP.Applicaton.Interfaces;
using System.Net.Http.Json;

namespace IETT_APP.Infrastructure.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _client;

        public ApiService(HttpClient client)
        {
            _client = client;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginUserDto dto)
        {
            var response = await _client.PostAsJsonAsync("/api/auth/login", dto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<AuthResponseDto>()
                ?? new AuthResponseDto();

        }

        public async Task<string> RegisterAsync(RegisterUserDto dto)
        {
            var response = await _client.PostAsJsonAsync("/api/auth/register", dto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}
