using IETT_APP.Application.Attributes;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace IETT_APP.WebAPI.Models
{
    public class CompleteProfileRequest
    {

        // 1. JSON String (Tüm veriler burada string olarak gelecek)
        [Required]
        [FromForm(Name = "data")] // Frontend "data" key'i ile gönderecek
        public string Data { get; set; } = string.Empty;

        // 2. Dosyalar
        [Required(ErrorMessage = "Ehliyet belgesi zorunludur.")]
        // Sadece PDF ve Word kabul et, içeriğini de kontrol et
        [AllowedExtensions(new[] { ".pdf", ".doc", ".docx" }, checkSignature: true)]
        public IFormFile? LicenseDocument { get; set; }

        [Required(ErrorMessage = "Psikoteknik belgesi zorunludur.")]
        [AllowedExtensions(new[] { ".pdf", ".doc", ".docx" }, checkSignature: true)]
        public IFormFile? PsychotechnicDocument { get; set; }
    }
}
