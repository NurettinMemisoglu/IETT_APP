using IETT_APP.Domain.Common;
using IETT_APP.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace IETT_APP.Domain.Entities
{
    public class Driver : BaseEntity<Guid>
    {
        // --- KİMLİK & KURUMSAL ---
        public string EmployeeNumber { get; set; } = string.Empty; // Sicil No
        public string? TCIdentityNumber { get; set; }
        public string? SocialSecurityNumber { get; set; } // SGK No (YENİ)
        public DriverType DriverType { get; set; } = DriverType.IETT_Staff; // (YENİ ENUM)
        public DateTime EmploymentDate { get; set; } // İşe Giriş (YENİ)

        // --- EHLİYET & BELGE ---
        public string? LicenseNumber { get; set; }
        public string? LicenseClass { get; set; }
        public DateTime? LicenseExpiryDate { get; set; }
        public string? SrcCertificateNumber { get; set; }
        public DateTime? PsychotechnicExpiryDate { get; set; }

        // --- İLETİŞİM & SAĞLIK ---
        public string? BloodType { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? Address { get; set; } // (YENİ)
        public bool HasChronicDisease { get; set; } // (YENİ)
        public string? HealthNotes { get; set; } // (YENİ - Hastalık detayları)

        // --- DURUM & DOSYA ---
        public WorkStatus WorkStatus { get; set; } = WorkStatus.OffDuty;
        public string? ProfileImagePath { get; set; }
        public string? LicenseDocumentPath { get; set; }      // Ehliyet Belgesi Yolu
        public string? PsychotechnicDocumentPath { get; set; } // Psikoteknik Belgesi Yolu

        // --- İLİŞKİLER ---
        public string UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        public Guid? GarageId { get; set; }
        [ForeignKey("GarageId")]
        public Garage<Guid>? Garage { get; set; }

        public ICollection<TripTask> TripTasks { get; set; } = new List<TripTask>();
    }
}