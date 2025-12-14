namespace IETT_APP.WebMVC.Areas.Admin.Models
{
    public class DashboardViewModel
    {
        public int TotalUserCount { get; set; }

        // Hangi rolden kaç kişi var? (Örn: "Admin": 2, "Driver": 50)
        public Dictionary<string, int> RoleCounts { get; set; } = new();
    }
}