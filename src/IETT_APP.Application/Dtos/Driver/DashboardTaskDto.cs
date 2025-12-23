using IETT_APP.Domain.Enums;

namespace IETT_APP.Application.Dtos.Driver
{
    public class DashboardTaskDto
    {
        public Guid Id { get; set; }

        // --- GÖRSEL & BAŞLIK BİLGİLERİ ---
        public string LineCode { get; set; }
        public string LineName { get; set; }
        public string RouteName { get; set; }
        public string VehiclePlate { get; set; }
        public string DoorNumber { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        // --- ZAMANLAMA (DÜZELTME BURADA) ---
        // Eski: public DateTime ScheduledTime { get; set; }

        // Yeni: Hem View hatasını çözer hem de mantığı karşılar.
        // Bu alana: AdjustedDeparture ?? ScheduledDeparture gelecek.
        public DateTime TaskDate { get; set; }

        public string FormattedTime { get; set; } // "14:30" gibi string formatı

        // --- DİĞER ALANLAR AYNEN KALIYOR ---
        public TaskState Status { get; set; }
        public string ActionButtonText { get; set; }
        public bool IsUrgent { get; set; }
        public int DelayMinutes { get; set; }
    }
}