using IETT_APP.Application.Interfaces;
using IETT_APP.Domain.Enums;
using IETT_APP.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace IETT_APP.Infrastructure.Services
{
    public class OperationJobService : IOperationJobService
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;
        private readonly ILogger<OperationJobService> _logger;
        private readonly IWebHostEnvironment _env; // <--- YENİ EKLENDİ

        public OperationJobService(
            AppDbContext context,
            INotificationService notificationService,
            IEmailService emailService,
            ILogger<OperationJobService> logger,
            IWebHostEnvironment env) // <--- Constructor'a eklendi
        {
            _context = context;
            _notificationService = notificationService;
            _emailService = emailService;
            _logger = logger;
            _env = env; // <--- Atama yapıldı
        }

        // ==================================================================
        // 1. MEVCUT: GECİKEN VE YAKLAŞAN SEFER KONTROLÜ
        // ==================================================================
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

                string driverName = task.Driver?.User?.Name ?? "Sürücü";
                string driverUserId = task.Driver?.User?.Id;
                string lineCode = task.Line?.Code ?? "???";
                string timeString = targetTime.Value.ToString("HH:mm");

                // 🛠️ DÜZELTME: LİNKLERİ AYRI AYRI OLUŞTURUYORUZ
                string chiefLink = $"/Chief/TripTasks/Details/{task.Id}";
                string driverLink = $"/Driver/Tasks/Details/{task.Id}";

                if (chiefUser != null)
                {
                    string realChiefId = chiefUser.Id;

                    // A) GECİKMİŞ SEFER (ERROR)
                    if (targetTime.Value <= now)
                    {
                        var delayMinutes = (int)(now - targetTime.Value).TotalMinutes;
                        string importance = "error";

                        // AMİR BİLDİRİMİ
                        string chiefTitle = "🚨 Kritik Gecikme Alarmı";
                        string chiefMessage = $"{lineCode} hattında, {timeString} seferi {delayMinutes} dakikadır başlatılmadı. Sorumlu Şoför: {driverName}.";
                        await _notificationService.SendNotificationAsync(realChiefId, chiefTitle, chiefMessage, importance, chiefLink);

                        // SÜRÜCÜ BİLDİRİMİ
                        if (!string.IsNullOrEmpty(driverUserId))
                        {
                            string driverTitle = "⚠️ Seferiniz Gecikiyor";
                            string driverMessage = $"Sayın {driverName}, {lineCode} hattındaki {timeString} seferiniz {delayMinutes} dakika gecikmeli görünüyor. Lütfen en kısa sürede çıkış yapınız.";
                            await _notificationService.SendNotificationAsync(driverUserId, driverTitle, driverMessage, importance, driverLink);
                        }
                    }
                    // B) YAKLAŞAN SEFER (WARNING)
                    else
                    {
                        var remainingMinutes = (int)(targetTime.Value - now).TotalMinutes;
                        string importance = "warning";

                        // AMİR BİLDİRİMİ
                        string chiefTitle = "⏳ Sefer Takibi";
                        string chiefMessage = $"{lineCode} - {driverName} seferine {remainingMinutes} dakika kaldı. Hazırlık durumu kontrol ediliyor.";
                        await _notificationService.SendNotificationAsync(realChiefId, chiefTitle, chiefMessage, importance, chiefLink);

                        // SÜRÜCÜ BİLDİRİMİ
                        if (!string.IsNullOrEmpty(driverUserId))
                        {
                            string driverTitle = "🚍 Hazırlık Zamanı";
                            string driverMessage = $"Merhaba {driverName}, {lineCode} hattı {timeString} seferinize {remainingMinutes} dakika kaldı. Aracınızı hazırlayıp perona yanaşabilirsiniz.";
                            await _notificationService.SendNotificationAsync(driverUserId, driverTitle, driverMessage, importance, driverLink);
                        }
                    }
                }
            }
        }

        // ==================================================================
        // 2. MEVCUT: OTOMATİK VARDİYA KAPATMA
        // ==================================================================
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

        // ==================================================================
        // 3. YENİ: ARAÇ VE EHLİYET MUAYENE KONTROLÜ (GÜNLÜK)
        // ==================================================================
        public async Task CheckExpirationsAsync()
        {
            var warningDate = DateTime.Today.AddDays(7); // 7 gün önceden uyarı

            // 1. Araç Sorgusu (Artık InspectionDate entity'de var)
            var riskyVehicles = await _context.Vehicles
                .Where(v => !v.IsDeleted &&
                           (v.InspectionDate <= warningDate || v.InsuranceDate <= warningDate))
                .Select(v => new { v.PlateNumber, v.InspectionDate, v.InsuranceDate })
                .ToListAsync();

            // 2. Sürücü Sorgusu (DÜZELTİLDİ: FullName yerine User tablosundan isim alındı)
            var riskyDrivers = await _context.Drivers
                .Include(d => d.User) // User tablosunu dahil et
                .Where(d => !d.IsDeleted && d.LicenseExpiryDate <= warningDate)
                .Select(d => new
                {
                    // HATA ÇÖZÜMÜ BURADA:
                    FullName = d.User.Name + " " + d.User.Surname,
                    d.LicenseExpiryDate
                })
                .ToListAsync();

            if (riskyVehicles.Any() || riskyDrivers.Any())
            {
                StringBuilder sb = new StringBuilder();

                if (riskyVehicles.Any())
                {
                    sb.AppendLine("Araçlar (Muayene/Sigorta Bitiyor):");
                    foreach (var v in riskyVehicles)
                    {
                        // Tarihler null olabilir, kontrol ederek yazdırıyoruz
                        var insp = v.InspectionDate.HasValue ? v.InspectionDate.Value.ToString("dd.MM.yyyy") : "-";
                        var insu = v.InsuranceDate.HasValue ? v.InsuranceDate.Value.ToString("dd.MM.yyyy") : "-";
                        sb.AppendLine($"- {v.PlateNumber} (Muayene: {insp}, Sigorta: {insu})");
                    }
                }

                if (riskyDrivers.Any())
                {
                    sb.AppendLine("\nSürücüler (Ehliyet Süresi Doluyor):");
                    foreach (var d in riskyDrivers)
                    {
                        sb.AppendLine($"- {d.FullName} (Tarih: {d.LicenseExpiryDate:dd.MM.yyyy})");
                    }
                }

                // Admin'i bul ve bildir
                var adminUser = await _context.UserRoles
                    .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur, r })
                    .Where(x => x.r.Name == "Admin" || x.r.Name == "Chief")
                    .Select(x => x.ur.UserId)
                    .FirstOrDefaultAsync();

                if (adminUser != null)
                {
                    await _notificationService.SendNotificationAsync(
                        adminUser,
                        "⚠️ Kritik: Süresi Dolan Kaynaklar",
                        sb.ToString(),
                        "warning",
                        "/Chief/Vehicles"
                    );
                }
            }
        }

        public async Task SendWeeklyReportAsync()
        {
            // 1. TARİH HESAPLAMALARI
            var today = DateTime.Today;
            var lastWeekStart = today.AddDays(-(int)today.DayOfWeek - 6);
            var lastWeekEnd = lastWeekStart.AddDays(7).AddSeconds(-1);

            // 2. VERİLERİ ÇEK
            var weeklyTasks = await _context.TripTasks
                .Include(t => t.Line)
                .Include(t => t.Driver).ThenInclude(d => d.User)
                .Where(t => t.ScheduledArrival >= lastWeekStart && t.ScheduledArrival <= lastWeekEnd)
                .AsNoTracking()
                .ToListAsync();

            if (!weeklyTasks.Any()) return;

            // 3. İSTATİSTİKLER
            int total = weeklyTasks.Count;
            int completed = weeklyTasks.Count(t => t.Status == TaskState.Completed);
            int cancelled = weeklyTasks.Count(t => t.Status == TaskState.Cancelled);
            double totalDelay = weeklyTasks.Sum(t => t.DelayInMinutes ?? 0);
            double successRate = total > 0 ? (double)completed / total * 100 : 0;

            // 4. HTML ŞABLONUNU DOSYADAN OKU
            string templatePath = Path.Combine(_env.WebRootPath, "Templates", "Email", "WeeklyReport.html");

            if (!System.IO.File.Exists(templatePath))
            {
                _logger.LogError($"Email şablonu bulunamadı: {templatePath}");
                return;
            }

            string templateContent = await System.IO.File.ReadAllTextAsync(templatePath);

            // 5. TABLO SATIRLARINI OLUŞTUR
            // A) En Çok Gecikenler
            var worstTrips = weeklyTasks
                .Where(t => t.DelayInMinutes > 0)
                .OrderByDescending(t => t.DelayInMinutes)
                .Take(5)
                .ToList();

            StringBuilder worstRows = new StringBuilder();

            if (worstTrips.Any())
            {
                foreach (var item in worstTrips)
                {
                    string driver = item.Driver?.User?.Name + " " + item.Driver?.User?.Surname ?? "-";
                    string line = item.Line?.Code ?? "?";

                    // --- HATA DÜZELTİLDİ: Nullable DateTime Kontrolü ---
                    string time = item.ScheduledDeparture.HasValue
                        ? item.ScheduledDeparture.Value.ToString("HH:mm")
                        : "-";
                    // --------------------------------------------------

                    worstRows.Append($"<tr><td><strong>{line}</strong></td><td>{driver}</td><td>{time}</td><td style='color:red'>+{item.DelayInMinutes} dk</td></tr>");
                }
            }
            else
            {
                worstRows.Append("<tr><td colspan='4' style='text-align:center'>Gecikme yok.</td></tr>");
            }

            // B) Yıldız Şoförler
            var topDrivers = weeklyTasks
                .Where(t => t.Status == TaskState.Completed && t.Driver != null)
                .GroupBy(t => t.DriverId)
                .Select(g => new
                {
                    Name = g.First().Driver.User.Name + " " + g.First().Driver.User.Surname,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(3)
                .ToList();

            StringBuilder topRows = new StringBuilder();
            if (topDrivers.Any())
            {
                int rank = 1;
                foreach (var d in topDrivers)
                {
                    topRows.Append($"<tr><td>{rank}.</td><td>{d.Name}</td><td style='color:green;font-weight:bold'>{d.Count}</td></tr>");
                    rank++;
                }
            }
            else
            {
                topRows.Append("<tr><td colspan='3'>Veri yok.</td></tr>");
            }

            // 6. ŞABLONDAKİ DEĞİŞKENLERİ DOLDUR
            string emailBody = templateContent
                .Replace("{{StartDate}}", lastWeekStart.ToString("dd.MM.yyyy"))
                .Replace("{{EndDate}}", lastWeekEnd.ToString("dd.MM.yyyy"))
                .Replace("{{TotalTrips}}", total.ToString())
                .Replace("{{CompletedTrips}}", completed.ToString())
                .Replace("{{CancelledTrips}}", cancelled.ToString())
                .Replace("{{TotalDelay}}", totalDelay.ToString())
                .Replace("{{SuccessRate}}", successRate.ToString("0.0"))
                .Replace("{{WorstTripsRows}}", worstRows.ToString())
                .Replace("{{TopDriversRows}}", topRows.ToString())
                .Replace("{{LinkUrl}}", "https://iett-panel.com/Chief/Reports"); // Linki kendine göre düzenle

            string plainMessage = $"Haftalık Rapor Hazır. Başarı Oranı: %{successRate:0.0}";

            // 7. ALICILARI BUL VE GÖNDER
            var rawRecipients = await _context.UserRoles
                  .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur, r })
                  .Join(_context.Users, temp => temp.ur.UserId, u => u.Id, (temp, u) => new { temp, u })
                  .Where(x => x.temp.r.Name == "Admin" || x.temp.r.Name == "Chief")
                  .Select(x => new { Id = x.u.Id, Email = x.u.Email })
                  .ToListAsync();

            var uniqueRecipients = rawRecipients
                  .Where(x => !string.IsNullOrEmpty(x.Email))
                  .GroupBy(x => x.Email).Select(g => g.First()).ToList();

            foreach (var user in uniqueRecipients)
            {
                try
                {
                    // Bildirim
                    await _notificationService.SendNotificationAsync(user.Id, "📊 Haftalık Rapor", plainMessage, "info", "/Chief/Reports");

                    // Email
                    await _emailService.SendEmailAsync(user.Email, $"Haftalık Rapor ({lastWeekStart:dd.MM} - {lastWeekEnd:dd.MM})", emailBody);

                    _logger.LogInformation($"{user.Email} adresine rapor gönderildi.");

                    // Mailtrap Limiti Bekleme
                    await Task.Delay(2000);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Mail hatası: {user.Email}");
                }
            }
        }
    }
}