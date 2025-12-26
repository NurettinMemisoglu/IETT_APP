using IETT_APP.Domain.Enums;

namespace IETT_APP.WebMVC.Areas.Chief.Models
{
    public class ChiefDashboardViewModel
    {
        // --- 1. KPI (Genel Bakış) ---
        public int TotalTasksToday { get; set; }
        public int ActiveTrips { get; set; }
        public int PendingIssues { get; set; }
        public int CompletedToday { get; set; }

        // --- 2. ZAMAN ÇİZELGESİ VERİLERİ ---
        public List<TimelineItemViewModel> DailyTimeline { get; set; } = new();

        // --- 3. SAĞ PANEL VERİLERİ ---
        public List<AlertViewModel> CriticalAlerts { get; set; } = new();
        public DriverStatusSummary DriverStats { get; set; } = new();
    }

    public class TimelineItemViewModel
    {
        public Guid Id { get; set; }
        public string LineCode { get; set; }
        public string RouteName { get; set; }
        public string DriverName { get; set; }
        public string PlateNumber { get; set; }
        public DateTime ScheduledTime { get; set; }
        public DateTime? ActualTime { get; set; }
        public TaskState Status { get; set; }
        public bool IsDelayed { get; set; }
        public int DelayMinutes { get; set; }
    }

    public class AlertViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Reason { get; set; }
        public DateTime Time { get; set; }
        public AlertType AlertType { get; set; }
    }

    public class DriverStatusSummary
    {
        public int TotalDrivers { get; set; }
        public int Active { get; set; }     // Direksiyon Başında
        public int Available { get; set; }  // Görev Bekleniyor
        public int OnLeave { get; set; }    // İzinli + Raporlu + İdari İzin
    }
}