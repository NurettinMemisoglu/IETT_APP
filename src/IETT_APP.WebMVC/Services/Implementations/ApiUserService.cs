// ApiUserService.cs (MVC/Services/Implementations)
using IETT_APP.Application.Dtos;
using IETT_APP.WebMVC.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace IETT_APP.WebMVC.Services.Implementations
{
    public class ApiUserService : IApiUserService
    {
        private readonly HttpClient _httpClient;

        public ApiUserService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            var users = await _httpClient.GetFromJsonAsync<IEnumerable<UserDto>>("api/users");
            return users ?? new List<UserDto>();
        }

        public async Task<IEnumerable<IdentityRole>> GetRolesAsync()
        {
            try
            {
                var roles = await _httpClient.GetFromJsonAsync<IEnumerable<IdentityRole>>("api/users/roles");
                return roles ?? new List<IdentityRole>();
            }
            catch
            {
                // Hata durumunda boş liste dön
                return new List<IdentityRole>();
            }
        }

        public async Task<UserDto?> GetByIdAsync(string id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<UserDto>($"api/users/{id}");
            }
            catch
            {
                return null;
            }
        }

        public async Task<(IdentityResult Result, UserDto? User)> CreateUserAsync(UserDto user, string password)
        {
            var url = $"api/users?password={Uri.EscapeDataString(password)}";
            var response = await _httpClient.PostAsJsonAsync(url, user);

            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                return (IdentityResult.Failed(new IdentityError { Description = errorMsg }), null);
            }

            var createdUser = await response.Content.ReadFromJsonAsync<UserDto>();
            return (IdentityResult.Success, createdUser);
        }

        public async Task<IdentityResult> DeleteUserAsync(string id)
        {
            var response = await _httpClient.DeleteAsync($"api/users/{id}");
            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                return IdentityResult.Failed(new IdentityError { Description = errorMsg });
            }
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> AddRoleAsync(string userId, string roleName)
        {
            var url = $"api/users/{userId}/roles/add?roleName={Uri.EscapeDataString(roleName)}";
            var response = await _httpClient.PostAsync(url, null);

            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                return IdentityResult.Failed(new IdentityError { Description = $"Rol eklenemedi: {errorMsg}" });
            }

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> RemoveRoleAsync(string userId, string roleName)
        {
            var url = $"api/users/{userId}/roles/remove?roleName={Uri.EscapeDataString(roleName)}";
            var response = await _httpClient.PostAsync(url, null);

            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                return IdentityResult.Failed(new IdentityError { Description = $"Rol kaldırılamadı: {errorMsg}" });
            }

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> CreateRoleAsync(string roleName)
        {
            var url = $"/api/roles/create?roleName={Uri.EscapeDataString(roleName)}";
            var response = await _httpClient.PostAsync(url, null);

            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                return IdentityResult.Failed(new IdentityError { Description = $"Rol oluşturulamadı: {errorMsg}" });
            }

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteRoleAsync(string roleId)
        {
            var url = $"/api/roles/{Uri.EscapeDataString(roleId)}";
            var response = await _httpClient.DeleteAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                return IdentityResult.Failed(new IdentityError { Description = $"Rol silinemedi: {errorMsg}" });
            }

            return IdentityResult.Success;
        }

    }
}
