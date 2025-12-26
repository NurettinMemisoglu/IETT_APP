using IETT_APP.Application.Dtos.Chief; // veya Dtos.Dashboard (DTO'nun yerine göre)
using IETT_APP.WebMVC.Areas.Chief.Models;

namespace IETT_APP.WebMVC.Areas.Chief.Extensions
{
    public static class ChiefDashboardMappingExtensions
    {
        public static ChiefDashboardViewModel ToViewModel(this ChiefDashboardDto dto)
        {
            if (dto == null) return new ChiefDashboardViewModel();

            return new ChiefDashboardViewModel
            {
                // 1. KPI Verileri
                TotalTasksToday = dto.TotalTasksToday,
                ActiveTrips = dto.ActiveTrips,
                CompletedToday = dto.CompletedToday,
                PendingIssues = dto.PendingIssues,

                // 2. Şoför İstatistikleri
                DriverStats = new DriverStatusSummary
                {
                    TotalDrivers = dto.DriverStats.TotalDrivers,
                    Active = dto.DriverStats.Active,
                    Available = dto.DriverStats.Available,
                    OnLeave = dto.DriverStats.OnLeave
                },

                // 3. Timeline Listesi
                DailyTimeline = dto.DailyTimeline?.Select(t => new TimelineItemViewModel
                {
                    Id = t.Id,
                    LineCode = t.LineCode,
                    RouteName = t.RouteName,
                    DriverName = t.DriverName,
                    PlateNumber = t.PlateNumber,
                    ScheduledTime = t.ScheduledTime,
                    ActualTime = t.ActualTime,
                    Status = t.Status,
                    IsDelayed = t.IsDelayed,
                    DelayMinutes = t.DelayMinutes
                }).ToList() ?? new List<TimelineItemViewModel>(),

                // 4. Kritik Uyarılar (GÜNCELLENEN KISIM)
                CriticalAlerts = dto.CriticalAlerts?.Select(a => new AlertViewModel
                {
                    Id = a.Id,
                    Title = a.Title,
                    Reason = a.Reason,
                    Time = a.Time,
                    AlertType = a.AlertType // <-- BURASI EKLENDİ
                }).ToList() ?? new List<AlertViewModel>()
            };
        }
    }
}