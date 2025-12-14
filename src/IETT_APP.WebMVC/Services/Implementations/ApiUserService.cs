using IETT_APP.Application.Dtos;
using IETT_APP.Application.Wrappers; // ServiceResult sınıfı burada
using IETT_APP.WebMVC.Areas.Admin.Models;
using IETT_APP.WebMVC.Services.Interfaces;
using System.Text.Json;

namespace IETT_APP.WebMVC.Services.Implementations
{
    public class ApiUserService : IApiUserService
    {
        private readonly HttpClient _httpClient;

        public ApiUserService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // --- GET İŞLEMLERİ ---

        public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            try
            {
                var users = await _httpClient.GetFromJsonAsync<IEnumerable<UserDto>>("api/users");
                return users ?? new List<UserDto>();
            }
            catch
            {
                return new List<UserDto>();
            }
        }

        public async Task<IEnumerable<RoleViewModel>> GetAllRolesAsync()
        {
            try
            {
                var roleNames = await _httpClient.GetFromJsonAsync<IEnumerable<string>>("api/users/roles");
                if (roleNames == null) return new List<RoleViewModel>();

                return roleNames.Select(r => new RoleViewModel { Id = r, Name = r }).ToList();
            }
            catch
            {
                return new List<RoleViewModel>();
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

        // --- İŞLEM METOTLARI (ServiceResult Döndürenler) ---

        public async Task<ServiceResult<UserDto>> CreateUserAsync(UserDto user, string password)
        {
            var url = $"api/users?password={Uri.EscapeDataString(password)}";
            var response = await _httpClient.PostAsJsonAsync(url, user);

            // Veri dönen işlemler için Generic HandleResponse kullanıyoruz
            return await HandleResponse<UserDto>(response);
        }
        public async Task<ServiceResult> UpdateUserAsync(UserDto user)
        {
            // API Endpoint: PUT api/users/{id}
            var url = $"api/users/{user.Id}";

            var response = await _httpClient.PutAsJsonAsync(url, user);

            return await HandleResponse(response);
        }

        public async Task<ServiceResult> DeleteUserAsync(string id)
        {
            var response = await _httpClient.DeleteAsync($"api/users/{id}");
            return await HandleResponse(response);
        }

        public async Task<ServiceResult> AddRoleAsync(string userId, string roleName)
        {
            var url = $"api/users/{userId}/roles/add?roleName={Uri.EscapeDataString(roleName)}";
            var response = await _httpClient.PostAsync(url, null);
            return await HandleResponse(response);
        }

        public async Task<ServiceResult> RemoveRoleAsync(string userId, string roleName)
        {
            var url = $"api/users/{userId}/roles/remove?roleName={Uri.EscapeDataString(roleName)}";
            var response = await _httpClient.PostAsync(url, null);
            return await HandleResponse(response);
        }

        public async Task<ServiceResult> CreateRoleAsync(string roleName)
        {
            var url = $"api/users/roles/create?roleName={Uri.EscapeDataString(roleName)}";

            var response = await _httpClient.PostAsync(url, null);
            return await HandleResponse(response);
        }

        public async Task<ServiceResult> DeleteRoleAsync(string roleName)
        {
            var url = $"api/users/roles/{Uri.EscapeDataString(roleName)}";

            var response = await _httpClient.DeleteAsync(url);
            return await HandleResponse(response);
        }

        // ========================================================================
        // 🔥 YARDIMCI METOTLAR (Kod Tekrarını Önler ve Hatayı Standartlaştırır)
        // ========================================================================

        // 1. Veri Dönmeyen İşlemler İçin (Delete, AddRole vb.)
        private async Task<ServiceResult> HandleResponse(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return ServiceResult.Success();
            }

            var errorMessage = await ExtractErrorMessage(response);
            return ServiceResult.Failure(errorMessage);
        }

        // 2. Veri Dönen İşlemler İçin (CreateUser vb.)
        private async Task<ServiceResult<T>> HandleResponse<T>(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var data = await response.Content.ReadFromJsonAsync<T>();
                    return ServiceResult<T>.Success(data!);
                }
                catch
                {
                    return ServiceResult<T>.Failure("İşlem başarılı ancak veri okunamadı.");
                }
            }

            var errorMessage = await ExtractErrorMessage(response);
            return ServiceResult<T>.Failure(errorMessage);
        }

        // 3. API'den Gelen Hatayı Ayıklama (JSON Parsing)
        private async Task<string> ExtractErrorMessage(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            try
            {
                using var doc = JsonDocument.Parse(content);

                // API'nin döndüğü olası hata formatlarını kontrol et:
                // 1. { "message": "Hata detayı" }
                if (doc.RootElement.TryGetProperty("message", out var msgElement))
                    return msgElement.GetString() ?? "Bilinmeyen hata";

                // 2. { "Message": "Hata detayı" } (Büyük harf)
                if (doc.RootElement.TryGetProperty("Message", out var msgElementBig))
                    return msgElementBig.GetString() ?? "Bilinmeyen hata";

                // 3. { "title": "One or more validation errors..." } (FluentValidation/Default Model Binder)
                if (doc.RootElement.TryGetProperty("title", out var titleElement))
                    return titleElement.GetString() + " (Detaylar için loglara bakınız)";

                return content; // JSON ama tanıdık format değilse raw dön
            }
            catch
            {
                // JSON değilse (örn: 500 Internal Server Error html sayfası veya düz yazı)
                return "Sunucu ile iletişimde hata oluştu. " + response.StatusCode;
            }
        }
    }
}