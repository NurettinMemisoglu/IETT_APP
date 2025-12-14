using IETT_APP.Domain.Enums;

namespace IETT_APP.Application.Dtos.Driver
{
    // Sürücü yasal ve operasyonel uygunluk kontrolü için kullanılır.
    public class DriverEligibilityDto
    {
        public Guid Id { get; set; }

        // Durum Kontrolü
        public WorkStatus WorkStatus { get; set; }
        public bool IsActive { get; set; }

        // Belge Kontrolü
        public DateTime? LicenseExpiryDate { get; set; }
        public DateTime? PsychotechnicExpiryDate { get; set; }

        // Dinlenme Süresi Kontrolü için son görevin bitişi
        public DateTime? LastCompletedTaskEndTime { get; set; }

        // Hata mesajları için (Opsiyonel)
        public string? Name { get; set; }
        public string? Surname { get; set; }

        // İzin başlangıç/bitiş tarihleri (Entity'de tanımlıysa buraya da eklenmeli)
    }
}