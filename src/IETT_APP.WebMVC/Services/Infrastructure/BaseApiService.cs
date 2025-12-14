using IETT_APP.Application.Wrappers;
using System.Text.Json;

namespace IETT_APP.WebMVC.Services.Infrastructure
{
    public abstract class BaseApiService
    {
        // 1. Veri Dönmeyen İşlemler (Delete, Update vb.)
        protected async Task<ServiceResult> HandleResponse(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return ServiceResult.Success();
            }

            var errorDetails = await ExtractErrorDetails(response);
            var result = ServiceResult.Failure(errorDetails.Errors);
            result.Message = errorDetails.Message;

            return result;
        }

        // 2. Veri Dönen İşlemler (Get, Create vb.)
        protected async Task<ServiceResult<T>> HandleResponse<T>(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                        return ServiceResult<T>.Success(default!);

                    var data = await response.Content.ReadFromJsonAsync<T>();
                    return ServiceResult<T>.Success(data!);
                }
                catch
                {
                    return ServiceResult<T>.Failure("Veri okunamadı veya format hatalı.");
                }
            }

            var errorDetails = await ExtractErrorDetails(response);
            var result = ServiceResult<T>.Failure(errorDetails.Errors);
            result.Message = errorDetails.Message;

            return result;
        }

        // 3. Ortak Hata Ayıklayıcı (ExceptionMiddleware Uyumlu)
        protected async Task<(string Message, List<string> Errors)> ExtractErrorDetails(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            var errors = new List<string>();
            var message = "İşlem başarısız.";

            try
            {
                // Boş içerik kontrolü
                if (string.IsNullOrWhiteSpace(content))
                {
                    return ($"Hata ({response.StatusCode})", new List<string> { "Sunucudan boş cevap döndü." });
                }

                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;

                // 1. Ana Mesajı Yakala (Öncelik Sırasına Göre)
                if (root.TryGetProperty("message", out var msg))
                    message = msg.GetString() ?? message;
                else if (root.TryGetProperty("Message", out var msgBig))
                    message = msgBig.GetString() ?? message;

                // .NET Standart ProblemDetails Formatı (title)
                else if (root.TryGetProperty("title", out var title))
                    message = title.GetString() ?? message;

                // Detail varsa mesaja ekle (Çok faydalıdır)
                if (root.TryGetProperty("detail", out var detail))
                {
                    var detailStr = detail.GetString();
                    if (!string.IsNullOrEmpty(detailStr))
                        message += $" ({detailStr})";
                }

                // 2. Hata Listesini Yakala (Validation Errors)
                if (root.TryGetProperty("errors", out var errorsElement))
                {
                    if (errorsElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in errorsElement.EnumerateArray())
                        {
                            if (item.ValueKind == JsonValueKind.Object && item.TryGetProperty("description", out var desc))
                                errors.Add(desc.GetString()!);
                            else
                                errors.Add(item.ToString());
                        }
                    }
                    else if (errorsElement.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var prop in errorsElement.EnumerateObject())
                        {
                            foreach (var err in prop.Value.EnumerateArray())
                            {
                                errors.Add(err.GetString() ?? "Hata");
                            }
                        }
                    }
                }
            }
            catch
            {
                // JSON değilse (HTML hata sayfası vb.)
                message = $"Sunucu Hatası ({response.StatusCode}): {content.Substring(0, Math.Min(content.Length, 100))}...";
                errors.Add(message);
            }

            if (!errors.Any()) errors.Add(message);

            return (message, errors);
        }
    }
}