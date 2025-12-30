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
        // --- DÜZENLENMİŞ COMPLETE PROFILE METODU ---
        public async Task<ServiceResult<DriverDto>> CompleteProfileAsync(string userId, CompleteProfileDto dto)
        {
            using var content = new MultipartFormDataContent();

            // 1. JSON Verisi
            var jsonString = JsonSerializer.Serialize(dto);
            content.Add(new StringContent(jsonString), "data");

            // 2. Dosyalar
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

            // 3. API İsteği
            var response = await _httpClient.PostAsync("api/drivers/complete-profile", content);

            // 4. BAŞARILI DURUM (GÜNCELLENDİ)
            if (response.IsSuccessStatusCode)
            {
                // Backend artık { Success, Message, RedirectUrl, Data } dönüyor.
                // Bunu doğrudan ServiceResult<DriverDto> olarak okumak yerine manuel mapliyoruz.
                var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>();

                bool isSuccess = false;
                // Backend "Success" gönderiyor, bunu yakala:
                if (jsonResponse.TryGetProperty("Success", out var successProp)) isSuccess = successProp.GetBoolean();
                else if (jsonResponse.TryGetProperty("success", out var successPropLower)) isSuccess = successPropLower.GetBoolean();

                string message = "";
                if (jsonResponse.TryGetProperty("Message", out var msgProp)) message = msgProp.GetString() ?? "";

                DriverDto? data = null;
                if (jsonResponse.TryGetProperty("Data", out var dataProp))
                {
                    // Data içerisindeki DriverDto'yu deserialize et
                    data = JsonSerializer.Deserialize<DriverDto>(dataProp.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }

                // MVC Controller'a dönecek standart ServiceResult oluştur
                return new ServiceResult<DriverDto>
                {
                    Succeeded = isSuccess, // Backend'deki "Success" buraya
                    Message = message,     // Backend'deki "Message" buraya
                    Data = data
                };
            }

            // 5. HATALI DURUM (AYNEN KORUNDU)
            var errorDetails = await ExtractErrorDetails(response);
            string rawError = (errorDetails.Message ?? "") + " " + (errorDetails.Errors != null ? string.Join(" ", errorDetails.Errors) : "");
            string userFriendlyMessage = "İşlem gerçekleştirilemedi.";

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
                userFriendlyMessage = !string.IsNullOrEmpty(errorDetails.Message) ? errorDetails.Message : "Sunucu tarafında bir hata oluştu.";
            }

            return new ServiceResult<DriverDto>
            {
                Succeeded = false,
                Message = userFriendlyMessage
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