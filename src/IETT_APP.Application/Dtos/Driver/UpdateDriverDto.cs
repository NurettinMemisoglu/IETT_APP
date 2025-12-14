using IETT_APP.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace IETT_APP.Application.Dtos.Driver
{
    public class UpdateDriverDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string EmployeeNumber { get; set; } = string.Empty;

        public DriverType DriverType { get; set; }
        public DateTime EmploymentDate { get; set; }

        public string? TCIdentityNumber { get; set; }
        public string? SocialSecurityNumber { get; set; }

        public string? LicenseNumber { get; set; }
        public string? LicenseClass { get; set; }
        public DateTime? LicenseExpiryDate { get; set; }
        public string? SrcCertificateNumber { get; set; }
        public DateTime? PsychotechnicExpiryDate { get; set; }

        public string? BloodType { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? Address { get; set; }
        public bool HasChronicDisease { get; set; }
        public string? HealthNotes { get; set; }

        public WorkStatus WorkStatus { get; set; }
        public bool IsActive { get; set; }
        public Guid? GarageId { get; set; }
    }
}