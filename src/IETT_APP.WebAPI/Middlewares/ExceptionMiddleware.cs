using System.Net;
using System.Text.Json;

namespace IETT_APP.WebAPI.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // İsteği bir sonraki adıma ilet
                await _next(context);
            }
            catch (Exception ex)
            {
                // Hata olursa yakala ve logla
                _logger.LogError(ex, ex.Message);

                // İstemciye (MVC) düzgün cevap dön
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            // Eğer hata bizim fırlattığımız özel bir hata ise (örn: "Kullanıcı bulunamadı"), 400 dönelim
            // (Burada Exception tipine göre ayrım yapılabilir)
            if (ex.Message.Contains("bulunamadı") || ex.Message.Contains("zaten mevcut") || ex.Message.Contains("Hata"))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }

            var response = new
            {
                statusCode = context.Response.StatusCode,
                message = ex.Message, // Hatanın asıl mesajı (Örn: "Sicil no benzersiz olmalı")
                detail = _env.IsDevelopment() ? ex.StackTrace?.ToString() : "Sunucu hatası." // Dev modunda detay göster
            };

            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(response, jsonOptions);

            await context.Response.WriteAsync(json);
        }
    }
}