using IETT_APP.Domain.Enums;

namespace IETT_APP.Application.Dtos.Driver
{
    public class DriverDto
    {
        public Guid Id { get; set; }

        // --- User Tablosundan ---
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;

        // --- Kimlik & Kurumsal ---
        public string EmployeeNumber { get; set; } = string.Empty;
        public string? TCIdentityNumber { get; set; }
        public string? SocialSecurityNumber { get; set; } // YENİ
        public DriverType DriverType { get; set; }       // YENİ
        public DateTime EmploymentDate { get; set; }     // YENİ

        // --- Ehliyet & Belge ---
        public string? LicenseNumber { get; set; }
        public string? LicenseClass { get; set; }
        public DateTime? LicenseExpiryDate { get; set; }
        public string? SrcCertificateNumber { get; set; }
        public DateTime? PsychotechnicExpiryDate { get; set; }

        public string? LicenseDocumentPath { get; set; }
        public string? PsychotechnicDocumentPath { get; set; }

        // --- Sağlık & İletişim ---
        public string? BloodType { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? Address { get; set; }            // YENİ
        public bool HasChronicDisease { get; set; }     // YENİ
        public string? HealthNotes { get; set; }        // YENİ

        // --- Durum ---
        public WorkStatus WorkStatus { get; set; }
        public bool IsActive { get; set; }
        public string? ProfileImagePath { get; set; }

        // --- Bağlı Olduğu Garaj ---
        public Guid? GarageId { get; set; }
        public string? GarageName { get; set; }

    }
}