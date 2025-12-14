using IETT_APP.Application.Attributes;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IETT_APP.Application.Dtos.Driver
{
    public class CompleteProfileDto
    {
        // --- Kurumsal Bilgiler ---
        [Required(ErrorMessage = "Sicil numarası zorunludur.")]
        public string EmployeeNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "TC Kimlik numarası zorunludur.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "TC Kimlik 11 haneli olmalıdır.")]
        public string TCIdentityNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "SGK numarası zorunludur.")]
        public string SocialSecurityNumber { get; set; } = string.Empty;

        // --- Ehliyet & Belgeler ---
        [Required(ErrorMessage = "Ehliyet numarası zorunludur.")]
        public string? LicenseNumber { get; set; }

        [Required(ErrorMessage = "Ehliyet sınıfı zorunludur.")]
        public string? LicenseClass { get; set; } // Örn: E, D1

        [Required(ErrorMessage = "Ehliyet geçerlilik tarihi zorunludur.")]
        public DateTime? LicenseExpiryDate { get; set; }

        public string? SrcCertificateNumber { get; set; }
        public DateTime? PsychotechnicExpiryDate { get; set; }

        // --- İletişim & Adres (YENİ) ---
        [Required(ErrorMessage = "Adres bilgisi zorunludur.")]
        public string? Address { get; set; }

        // --- Sağlık Bilgileri (YENİ) ---
        [Required(ErrorMessage = "Kan grubu zorunludur.")]
        public string? BloodType { get; set; }

        public bool HasChronicDisease { get; set; } = false; // Kronik rahatsızlık var mı?
        public string? HealthNotes { get; set; } // Varsa detayları

        // --- Acil Durum ---
        [Required(ErrorMessage = "Acil durum kişisi zorunludur.")]
        public string? EmergencyContactName { get; set; }

        [Required(ErrorMessage = "Acil durum telefonu zorunludur.")]
        [Phone]
        public string? EmergencyContactPhone { get; set; }

        [JsonIgnore] // <--- KRİTİK NOKTA: Serialize ederken bunları atla
        [Required(ErrorMessage = "Ehliyet belgesi zorunludur.")]
        [AllowedExtensions(new[] { ".pdf", ".doc", ".docx" }, checkSignature: true)]
        public IFormFile? LicenseDocument { get; set; }

        [JsonIgnore] // <--- KRİTİK NOKTA
        [Required(ErrorMessage = "Psikoteknik belgesi zorunludur.")]
        [AllowedExtensions(new[] { ".pdf", ".doc", ".docx" }, checkSignature: true)]
        public IFormFile? PsychotechnicDocument { get; set; }
    }
}