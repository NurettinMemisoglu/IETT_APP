using IETT_APP.Application.Interfaces;
using IETT_APP.Domain.Enums;
using IETT_APP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IETT_APP.Infrastructure.Services
{
    public class OperationJobService : IOperationJobService
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly ILogger<OperationJobService> _logger;

        public OperationJobService(
            AppDbContext context,
            INotificationService notificationService,
            ILogger<OperationJobService> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task CheckDelayedTripsAsync()
        {
            var now = DateTime.Now;
            var upcomingThreshold = now.AddMinutes(5);
            var pastThreshold = now.AddMinutes(-5);

            var tasks = await _context.TripTasks
                .Include(t => t.Line)
                .Include(t => t.Driver).ThenInclude(d => d.User)
                .Where(t => (t.Status == TaskState.Pending || t.Status == TaskState.Accepted)
                            && !t.IsDeleted
                            && (t.AdjustedDeparture ?? t.ScheduledDeparture) <= upcomingThreshold
                            && (t.AdjustedDeparture ?? t.ScheduledDeparture) >= pastThreshold)
                .ToListAsync();

            if (!tasks.Any()) return;

            foreach (var task in tasks)
            {
                var targetTime = task.AdjustedDeparture ?? task.ScheduledDeparture;
                if (!targetTime.HasValue) continue;

                // --- KULLANICI BİLGİLERİ ---
                string rawId = task.CreatedBy;
                var chiefUser = await _context.Users
                    .Where(u => u.Id == rawId || u.Email == rawId || u.UserName == rawId)
                    .Select(u => new { u.Id, u.Email })
                    .FirstOrDefaultAsync();

                // Şoför ismini al, yoksa varsayılan ata
                string driverName = task.Driver?.User?.Name ?? "Sürücü";
                string driverUserId = task.Driver?.User?.Id;
                string lineCode = task.Line?.Code ?? "???";
                string timeString = targetTime.Value.ToString("HH:mm");

                string detailLink = $"/Chief/TripTasks/Details/{task.Id}";

                if (chiefUser != null)
                {
                    string realChiefId = chiefUser.Id;

                    // ============================================================
                    // A) GECİKMİŞ SEFER (ERROR)
                    // ============================================================
                    if (targetTime.Value <= now)
                    {
                        var delayMinutes = (int)(now - targetTime.Value).TotalMinutes;
                        string importance = "error";

                        // --- 1. ŞEF İÇİN MESAJ (Denetim Odaklı) ---
                        string chiefTitle = "🚨 Kritik Gecikme Alarmı";
                        string chiefMessage = $"{lineCode} hattında, {timeString} seferi {delayMinutes} dakikadır başlatılmadı. " +
                                              $"Sorumlu Şoför: {driverName}. Müdahale ediniz.";

                        await _notificationService.SendNotificationAsync(realChiefId, chiefTitle, chiefMessage, importance, detailLink);

                        // --- 2. ŞOFÖR İÇİN MESAJ (Uyarı/Aksiyon Odaklı) ---
                        if (!string.IsNullOrEmpty(driverUserId))
                        {
                            string driverTitle = "⚠️ Seferiniz Gecikiyor";
                            string driverMessage = $"Sayın {driverName}, {lineCode} hattındaki {timeString} seferiniz {delayMinutes} dakika gecikmeli görünüyor. " +
                                                   $"Lütfen en kısa sürede çıkış yapınız.";

                            await _notificationService.SendNotificationAsync(driverUserId, driverTitle, driverMessage, importance, detailLink);
                        }
                    }
                    // ============================================================
                    // B) YAKLAŞAN SEFER (WARNING)
                    // ============================================================
                    else
                    {
                        var remainingMinutes = (int)(targetTime.Value - now).TotalMinutes;
                        string importance = "warning";

                        // --- 1. ŞEF İÇİN MESAJ (Bilgilendirme) ---
                        string chiefTitle = "⏳ Sefer Takibi";
                        string chiefMessage = $"{lineCode} - {driverName} seferine {remainingMinutes} dakika kaldı. Hazırlık durumu kontrol ediliyor.";

                        await _notificationService.SendNotificationAsync(realChiefId, chiefTitle, chiefMessage, importance, detailLink);

                        // --- 2. ŞOFÖR İÇİN MESAJ (Motivasyon/Hatırlatma) ---
                        if (!string.IsNullOrEmpty(driverUserId))
                        {
                            string driverTitle = "🚍 Hazırlık Zamanı";
                            string driverMessage = $"Merhaba {driverName}, {lineCode} hattı {timeString} seferinize {remainingMinutes} dakika kaldı. " +
                                                   $"Aracınızı hazırlayıp perona yanaşabilirsiniz. İyi yolculuklar dileriz. 👋";

                            await _notificationService.SendNotificationAsync(driverUserId, driverTitle, driverMessage, importance, detailLink);
                        }
                    }
                }
            }
        }

        public async Task AutoCloseShiftAsync()
        {
            var cutoffTime = DateTime.Today;
            var staleTasks = await _context.TripTasks
                .Where(t => (t.AdjustedDeparture ?? t.ScheduledDeparture) < cutoffTime
                      && (t.Status == TaskState.Pending || t.Status == TaskState.Accepted || t.Status == TaskState.InProgress)
                      && !t.IsDeleted)
                .ToListAsync();

            if (!staleTasks.Any()) return;

            foreach (var task in staleTasks)
            {
                task.Status = TaskState.Incomplete;
                task.StatusReason = "Sistem: Gün sonunda tamamlanmadığı için otomatik sonlandırıldı.";
                task.UpdatedAt = DateTime.UtcNow;
                task.UpdatedBy = "System_Job";
            }
            await _context.SaveChangesAsync();
        }
    }
}