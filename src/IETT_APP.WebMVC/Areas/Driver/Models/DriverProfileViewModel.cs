using IETT_APP.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace IETT_APP.WebMVC.Areas.Driver.Models
{
    public class DriverProfileViewModel
    {
        public Guid Id { get; set; } // Driver ID
        public string UserId { get; set; } = string.Empty;

        // --- Salt Okunur Bilgiler (ReadOnly) ---
        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Sicil No")]
        public string EmployeeNumber { get; set; } = string.Empty;

        [Display(Name = "E-Posta")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Garaj")]
        public string GarageName { get; set; } = "Atanmadı";

        [Display(Name = "Çalışma Durumu")]
        public WorkStatus WorkStatus { get; set; }

        // Sadece resmin yolunu (URL) tutuyoruz, dosyanın kendisini değil.
        public string? ProfileImagePath { get; set; }

        // --- Düzenlenebilir Kişisel Bilgiler ---
        [Required(ErrorMessage = "Telefon zorunludur.")]
        [Phone]
        [Display(Name = "Telefon")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "Adres")]
        public string? Address { get; set; }

        [Display(Name = "Acil Durum Kişisi")]
        public string? EmergencyContactName { get; set; }

        [Display(Name = "Acil Durum Telefonu")]
        [Phone]
        public string? EmergencyContactPhone { get; set; }

        // --- Sağlık Bilgileri ---
        [Display(Name = "Kan Grubu")]
        public string? BloodType { get; set; }

        [Display(Name = "Kronik Rahatsızlık")]
        public bool HasChronicDisease { get; set; }

        [Display(Name = "Sağlık Notları")]
        public string? HealthNotes { get; set; }

        public string? SrcCertificateNumber { get; set; }

        // --- Belge Bilgileri (Sadece Gösterim) ---
        public string? LicenseNumber { get; set; }
        public string? LicenseClass { get; set; }
        public DateTime? LicenseExpiryDate { get; set; }
        public DateTime? PsychotechnicExpiryDate { get; set; }

        public string? LicenseDocumentPath { get; set; }
        public string? PsychotechnicDocumentPath { get; set; }
    }
}