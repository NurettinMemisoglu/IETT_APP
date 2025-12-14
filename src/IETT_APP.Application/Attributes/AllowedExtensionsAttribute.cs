using IETT_APP.Application.Validators; // Namespace'i ekle
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace IETT_APP.Application.Attributes
{
    public class AllowedExtensionsAttribute : ValidationAttribute
    {
        private readonly string[] _extensions;

        // Hangi modda çalışacağını seçebiliriz (ImageOnly, DocumentOnly, Custom)
        private readonly bool _checkSignature;

        public AllowedExtensionsAttribute(string[] extensions, bool checkSignature = true)
        {
            _extensions = extensions;
            _checkSignature = checkSignature;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                // 1. Basit Uzantı Kontrolü
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!_extensions.Contains(ext))
                {
                    return new ValidationResult($"İzin verilen formatlar: {string.Join(", ", _extensions)}");
                }

                // 2. Güvenli İmza (Magic Number) Kontrolü
                if (_checkSignature)
                {
                    // Resim mi Belge mi olduğunu uzantıdan anlayıp ilgili metodu çağırabiliriz
                    // Veya yazdığımız genel 'ValidateSignature' mantığı çalışır.

                    bool isSignatureValid = false;

                    if (new[] { ".jpg", ".jpeg", ".png" }.Contains(ext))
                    {
                        isSignatureValid = file.IsValidImage();
                    }
                    else if (new[] { ".pdf", ".doc", ".docx" }.Contains(ext))
                    {
                        isSignatureValid = file.IsValidDocument();
                    }
                    else
                    {
                        // Listemizde imzası tanımlı olmayan bir dosya ise (örn: .txt) sadece uzantıya güveniriz
                        isSignatureValid = true;
                    }

                    if (!isSignatureValid)
                    {
                        return new ValidationResult("Dosya içeriği geçersiz veya bozuk (Uzantı ile içerik uyuşmuyor).");
                    }
                }
            }
            return ValidationResult.Success;
        }
    }
}