using IETT_APP.Domain.Enums;

namespace IETT_APP.Application.Dtos.Driver
{
    public class DriverDashboardDto
    {
        // --- 1. ŞOFÖR DURUMU (Header/Profil Alanı İçin) ---
        public string FullName { get; set; } = string.Empty;
        public string ProfileImageUrl { get; set; } = string.Empty;
        public WorkStatus WorkStatus { get; set; } // Enum: Available, Working, Resting

        // Mesai Durumu Metni (Örn: "Mesai Başladı - 08:30" veya "Molasız")
        public string ShiftStatusText { get; set; } = string.Empty;

        // --- 2. HERO KART (En Yakın / Aktif Görev) ---
        // Eğer null ise "Şu an aktif görev yok" ekranı gösterilir.
        public DashboardTaskDto? CurrentTask { get; set; }

        // --- 3. TIMELINE (Günün Geri Kalanı) ---
        // Liste boşsa "Bugün başka görev yok" denir.
        public List<DashboardTaskDto> UpcomingTasks { get; set; } = new();

        // --- 4. ÖZET İSTATİSTİK (Opsiyonel - Alt Bar İçin) ---
        public int CompletedTasksCount { get; set; } // Bugün tamamlanan sefer sayısı
    }
}