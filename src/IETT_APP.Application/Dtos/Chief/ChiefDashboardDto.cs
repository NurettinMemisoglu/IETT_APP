using IETT_APP.Domain.Enums;

namespace IETT_APP.Application.Dtos.Chief
{
    public class ChiefDashboardDto
    {
        // KPI Sayıları
        public int TotalTasksToday { get; set; }
        public int ActiveTrips { get; set; }
        public int PendingIssues { get; set; }
        public int CompletedToday { get; set; }

        // Listeler
        public List<DashboardTimelineItemDto> DailyTimeline { get; set; } = new();
        public List<DashboardAlertDto> CriticalAlerts { get; set; } = new();
        public DashboardDriverStatsDto DriverStats { get; set; } = new();
    }

    public class DashboardTimelineItemDto
    {
        public Guid Id { get; set; }
        public string LineCode { get; set; }      // TripTaskDto'yu kirletmeden buraya ekledik
        public string RouteName { get; set; }
        public string DriverName { get; set; }
        public string PlateNumber { get; set; }   // TripTaskDto'yu kirletmeden buraya ekledik
        public DateTime ScheduledTime { get; set; }
        public DateTime? ActualTime { get; set; }
        public TaskState Status { get; set; }
        public bool IsDelayed { get; set; }
        public int DelayMinutes { get; set; }
    }

    public class DashboardAlertDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Reason { get; set; }
        public DateTime Time { get; set; }
        public AlertType AlertType { get; set; }
    }

    public class DashboardDriverStatsDto
    {
        public int TotalDrivers { get; set; }
        public int Active { get; set; }
        public int Available { get; set; }
        public int OnLeave { get; set; }
    }
}
