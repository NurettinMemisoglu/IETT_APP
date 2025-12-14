using IETT_APP.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace IETT_APP.Infrastructure.Services
{
    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly IWebHostEnvironment _env;

        public EmailTemplateService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> GenerateEmailBodyAsync(string templateName, Dictionary<string, string> placeholders)
        {
            // 1. Dosya Yolunu Bul (WebAPI/wwwroot/Templates/Email/...)
            string webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            string templatePath = Path.Combine(webRootPath, "Templates", "Email", templateName);

            if (!File.Exists(templatePath))
            {
                // Fallback: Eğer dosya yoksa basit bir text dön veya logla
                return $"Mail içeriği oluşturulamadı. Şablon bulunamadı: {templateName}";
            }

            // 2. HTML İçeriğini Oku
            string body = await File.ReadAllTextAsync(templatePath);

            // 3. Yer Tutucuları (Placeholders) Değiştir
            foreach (var item in placeholders)
            {
                // HTML içindeki {{Key}} değerlerini Value ile değiştirir
                body = body.Replace($"{{{{{item.Key}}}}}", item.Value);
            }

            return body;
        }
    }
}