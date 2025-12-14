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
            // ID'yi URL'e veya Body'ye eklememize gerek yok.
            // Token (Header) zaten kim olduğunu söyleyecek.
            var response = await _httpClient.PatchAsJsonAsync("api/drivers/profile", dto);

            return await HandleResponse(response);
        }

        // --- ÖZEL İŞLEMLER ---
        public async Task<ServiceResult> AssignGarageAsync(AssignGarageDto dto)
        {
            var response = await _httpClient.PatchAsJsonAsync("api/drivers/assign-garage", dto);
            return await HandleResponse(response);
        }

        public async Task<ServiceResult<DriverDto>> CompleteProfileAsync(string userId, CompleteProfileDto dto)
        {
            using var content = new MultipartFormDataContent();

            // 1. DTO'yu JSON String'e Çevir (JsonIgnore sayesinde dosyalar hariç tutulur)
            var jsonString = JsonSerializer.Serialize(dto);

            // API "data" isminde bir form alanı bekliyor
            content.Add(new StringContent(jsonString), "data");


            // 2. Dosyaları Manuel Ekle

            // Ehliyet
            if (dto.LicenseDocument != null)
            {
                var fileContent = new StreamContent(dto.LicenseDocument.OpenReadStream());
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(dto.LicenseDocument.ContentType);
                content.Add(fileContent, "licenseDocument", dto.LicenseDocument.FileName);
            }

            // Psikoteknik
            if (dto.PsychotechnicDocument != null)
            {
                var fileContent = new StreamContent(dto.PsychotechnicDocument.OpenReadStream());
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(dto.PsychotechnicDocument.ContentType);
                content.Add(fileContent, "psychotechnicDocument", dto.PsychotechnicDocument.FileName);
            }

            // 3. İsteği Gönder
            var response = await _httpClient.PostAsync("api/drivers/complete-profile", content);

            return await HandleResponse<DriverDto>(response);
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

                    return ServiceResult<string>.Success("");
                }
                catch
                {
                    return ServiceResult<string>.Success("");
                }
            }

            // --- HATA YÖNETİMİ (DÜZELTİLDİ) ---
            var errorDetails = await ExtractErrorDetails(response);

            var result = ServiceResult<string>.Failure(errorDetails.Errors);
            result.Message = errorDetails.Message;

            return result;
        }
    }
}