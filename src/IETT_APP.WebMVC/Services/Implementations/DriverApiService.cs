using IETT_APP.Application.Dtos.Driver;
using IETT_APP.Application.Wrappers;
using IETT_APP.WebMVC.Services.Infrastructure; // BaseApiService
using IETT_APP.WebMVC.Services.Interfaces;
using System.Net.Http.Headers;
using System.Text.Json;

namespace IETT_APP.WebMVC.Services.Implementations
{
    public class DriverApiService : BaseApiService, IDriverApiService
    {
        private readonly HttpClient _httpClient;

        public DriverApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // --- GET METOTLARI ---
        public async Task<IEnumerable<DriverDto>> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<DriverDto>>("api/drivers")
                    ?? new List<DriverDto>();
        }

        public async Task<DriverDto?> GetByIdAsync(Guid id)
        {
            var response = await _httpClient.GetAsync($"api/drivers/{id}");
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<DriverDto>();
            return null;
        }

        public async Task<DriverDto?> GetByUserIdAsync(string userId)
        {
            var response = await _httpClient.GetAsync($"api/drivers/user/{userId}");
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<DriverDto>();
            return null;
        }

        public async Task<IEnumerable<DriverDto>> GetUnassignedDriversAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<DriverDto>>("api/drivers/unassigned")
                    ?? new List<DriverDto>();
        }

        // --- CREATE / UPDATE / DELETE ---
        public async Task<ServiceResult<DriverDto>> CreateAsync(CreateDriverDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/drivers", dto);
            return await HandleResponse<DriverDto>(response);
        }

        public async Task<ServiceResult> UpdateAsync(UpdateDriverDto dto)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/drivers/{dto.Id}", dto);
            return await HandleResponse(response);
        }

        public async Task<ServiceResult> DeleteAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"api/drivers/{id}");
            return await HandleResponse(response);
        }

        public async Task<ServiceResult> UpdateProfileAsync(UpdateDriverProfileDto dto)
        {
            var response = await _httpClient.PatchAsJsonAsync("api/drivers/profile", dto);
            return await HandleResponse(response);
        }

        // --- ÖZEL İŞLEMLER ---
        public async Task<ServiceResult> AssignGarageAsync(AssignGarageDto dto)
        {
            var response = await _httpClient.PatchAsJsonAsync("api/drivers/assign-garage", dto);
            return await HandleResponse(response);
        }

        // --- DÜZENLEME YAPILAN METOT ---
        public async Task<ServiceResult<DriverDto>> CompleteProfileAsync(string userId, CompleteProfileDto dto)
        {
            using var content = new MultipartFormDataContent();

            // 1. Veri Hazırlama (JSON + Dosyalar)
            var jsonString = JsonSerializer.Serialize(dto);
            content.Add(new StringContent(jsonString), "data");

            if (dto.LicenseDocument != null)
            {
                var fileContent = new StreamContent(dto.LicenseDocument.OpenReadStream());
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(dto.LicenseDocument.ContentType);
                content.Add(fileContent, "licenseDocument", dto.LicenseDocument.FileName);
            }

            if (dto.PsychotechnicDocument != null)
            {
                var fileContent = new StreamContent(dto.PsychotechnicDocument.OpenReadStream());
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(dto.PsychotechnicDocument.ContentType);
                content.Add(fileContent, "psychotechnicDocument", dto.PsychotechnicDocument.FileName);
            }

            // 2. API İsteği
            var response = await _httpClient.PostAsync("api/drivers/complete-profile", content);

            // 3. BAŞARILI DURUM
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ServiceResult<DriverDto>>();
            }

            // 4. HATALI DURUM (Mesajı Burada Güzelleştiriyoruz)
            var errorDetails = await ExtractErrorDetails(response);

            // API'den gelen ham hata mesajlarını birleştir
            string rawError = (errorDetails.Message ?? "") + " " + (errorDetails.Errors != null ? string.Join(" ", errorDetails.Errors) : "");
            string userFriendlyMessage = "İşlem gerçekleştirilemedi.";

            // --- KİRLİ İŞİ BURADA YAPIYORUZ (Controller Temiz Kalsın) ---
            if (rawError.Contains("IX_Drivers_EmployeeNumber") || rawError.Contains("EmployeeNumber"))
            {
                userFriendlyMessage = $"Girdiğiniz Sicil Numarası ({dto.EmployeeNumber}) sistemde zaten kayıtlı.";
            }
            else if (rawError.Contains("IX_Drivers_TCKN") || rawError.Contains("TCIdentityNumber"))
            {
                userFriendlyMessage = $"Girdiğiniz TC Kimlik No ({dto.TCIdentityNumber}) sistemde zaten kayıtlı.";
            }
            else if (rawError.ToLower().Contains("duplicate"))
            {
                userFriendlyMessage = "Bu bilgilerle daha önce kayıt yapılmış.";
            }
            else
            {
                // Eğer özel bir SQL hatası değilse, API'nin kendi mesajını kullan
                userFriendlyMessage = !string.IsNullOrEmpty(errorDetails.Message) ? errorDetails.Message : "Sunucu tarafında bir hata oluştu.";
            }

            // Controller'a tertemiz bir hata mesajı dönüyoruz
            return new ServiceResult<DriverDto>
            {
                Succeeded = false,
                Message = userFriendlyMessage // <-- ARTIK BU MESAJ TEMİZ
            };
        }

        // --- DOSYA YÜKLEME ---
        public async Task<ServiceResult<string>> UploadProfileImageAsync(Guid driverId, IFormFile photo)
        {
            using var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(photo.OpenReadStream());
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(photo.ContentType);

            content.Add(fileContent, "photo", photo.FileName);

            var response = await _httpClient.PostAsync($"api/drivers/{driverId}/upload-photo", content);

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var resultJson = await response.Content.ReadFromJsonAsync<JsonElement>();
                    if (resultJson.TryGetProperty("path", out var pathEl))
                        return ServiceResult<string>.Success(pathEl.GetString()!);

                    // API bazen direkt string, bazen obje dönebilir, yapına göre:
                    if (resultJson.TryGetProperty("data", out var dataEl))
                        return ServiceResult<string>.Success(dataEl.GetString()!);

                    return ServiceResult<string>.Success("");
                }
                catch
                {
                    return ServiceResult<string>.Success("");
                }
            }

            var errorDetails = await ExtractErrorDetails(response);
            var result = ServiceResult<string>.Failure(errorDetails.Errors);
            result.Message = errorDetails.Message;

            return result;
        }

        public async Task<DriverDashboardDto?> GetDashboardAsync()
        {
            var response = await _httpClient.GetAsync("api/drivers/dashboard");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<DriverDashboardDto>();
            }
            return null;
        }
    }
}