using IETT_APP.Application.Dtos.Driver; // <--- API'deki DTO'yu buradan çekiyoruz

namespace IETT_APP.WebMVC.Areas.Driver.Models
{
    public class DriverDashboardViewModel
    {
        // Profil var mı kontrolü
        public bool HasProfile { get; set; }

        // Profil Detayları
        public DriverDto? Profile { get; set; }

        // API'den gelen "Akıllı Dashboard" verisi
        // (FullName, ShiftStatus, CurrentTask, UpcomingTasks hepsi bunun içinde)
        public DriverDashboardDto DashboardData { get; set; } = new();
    }
}