using System.ComponentModel.DataAnnotations;

namespace IETT_APP.Application.Dtos.Driver
{
    public class UpdateDriverProfileDto
    {
        // --- İLETİŞİM ---

        [Required(ErrorMessage = "Telefon numarası zorunludur.")]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty; // User tablosuna gidecek

        public string? Address { get; set; }

        // --- ACİL DURUM ---

        public string? EmergencyContactName { get; set; }

        [Phone]
        public string? EmergencyContactPhone { get; set; }

        // --- SAĞLIK ---

        [Required(ErrorMessage = "Kan grubu zorunludur.")]
        public string? BloodType { get; set; }

        public bool HasChronicDisease { get; set; }

        public string? HealthNotes { get; set; }
    }
}