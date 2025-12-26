using AutoMapper;
using IETT_APP.Application.Dtos.Chief;
using IETT_APP.Application.Dtos.TripTask;
using IETT_APP.Application.Interfaces;
using IETT_APP.Domain.Entities;
using IETT_APP.Domain.Enums;
using IETT_APP.Domain.Interfaces;
using IETT_APP.Domain.Services;
using IETT_APP.Infrastructure.Hubs;
using IETT_APP.Infrastructure.Persistence; // AppDbContext için
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace IETT_APP.Infrastructure.Services
{
    public class TripTaskService : ITripTaskService
    {
        private readonly ITripTaskRepository _repository;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context; // Transaction için
        private readonly ILogger<TripTaskService> _logger;

        // Domain Servisleri
        private readonly TripTaskDomainService _tripTaskDomainService;
        private readonly DriverDomainService _driverDomainService;

        // Bildirim ve Veri Servisleri
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;
        private readonly IDriverRepository _driverRepository;
        private readonly IVehicleRepository<Guid> _vehicleRepository; // Eklendi
        private readonly IEmailTemplateService _templateService;
        private readonly IHubContext<NotificationHub, INotificationClient> _hubContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TripTaskService(
            ITripTaskRepository repository,
            IMapper mapper,
            UserManager<User> userManager,
            AppDbContext context,
            TripTaskDomainService tripTaskDomainService,
            DriverDomainService driverDomainService,
            IHttpContextAccessor httpContextAccessor,
            IEmailService emailService,
            INotificationService notificationService,
            IDriverRepository driverRepository,
            IVehicleRepository<Guid> vehicleRepository,
            IEmailTemplateService templateService,
            IHubContext<NotificationHub, INotificationClient> hubContext,
            ILogger<TripTaskService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _userManager = userManager;
            _context = context;
            _tripTaskDomainService = tripTaskDomainService;
            _driverDomainService = driverDomainService;
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService;
            _notificationService = notificationService;
            _driverRepository = driverRepository;
            _vehicleRepository = vehicleRepository;
            _templateService = templateService;
            _hubContext = hubContext;
            _logger = logger;
        }

        private string GetCurrentUserIdOrName()
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
            return userId ?? userName ?? "System";
        }

        // ============================================================
        // YÖNETİM METOTLARI (AMİR/CHIEF İÇİN)
        // ============================================================

        public async Task<Guid> AddAsync(TripTaskCreateDto dto)
        {
            // Validasyonlar
            if (!dto.ScheduledDeparture.HasValue || !dto.ScheduledArrival.HasValue)
                throw new Exception("Planlanan kalkış ve varış saatleri zorunludur.");

            if (dto.ScheduledArrival <= dto.ScheduledDeparture)
            {
                throw new Exception("Planlanan varış saati, kalkış saatinden sonra olmalıdır.");
            }

            await _tripTaskDomainService.ValidateRouteLineMatchAsync(dto.RouteId, dto.LineId);
            await _tripTaskDomainService.ValidateVehicleGarageMatchAsync(dto.VehicleId, dto.GarageId);

            if (dto.VehicleId.HasValue)
                await _tripTaskDomainService.ValidateVehicleAvailabilityAsync(dto.VehicleId.Value);

            await _tripTaskDomainService.ValidateAssignmentConflictAsync(
                dto.DriverId, dto.VehicleId, dto.ScheduledDeparture.Value, dto.ScheduledArrival.Value);

            if (dto.DriverId.HasValue)
            {
                await _driverDomainService.ValidateOperatorEligibilityAsync(dto.DriverId.Value, dto.ScheduledDeparture.Value);
            }

            var entity = _mapper.Map<TripTask>(dto);
            entity.Id = Guid.NewGuid();
            entity.Status = TaskState.Pending;
            entity.PassengerCount = 0;

            await _repository.AddAsync(entity);

            if (dto.DriverId.HasValue)
            {
                await SendAssignmentNotificationAsync(entity, dto);
            }

            // 🔥 EKLENEN SATIR: Sayfanın yenilenmesi için sinyal gönder
            await NotifyStateChange(entity.Id);

            return entity.Id;
        }

        public async Task UpdateAsync(TripTaskUpdateDto dto)
        {
            var entity = await _repository.GetByIdAsync(dto.Id);
            if (entity == null) throw new Exception("Görev bulunamadı.");

            var oldStatus = entity.Status;
            var oldDriverId = entity.DriverId;
            var oldVehicleId = entity.VehicleId;

            // --- DEĞİŞİKLİK 1: Eski saatlerin ikisini de hafızaya alıyoruz ---
            var oldStart = entity.ScheduledDeparture; // İlk plan
            var oldAdjStart = entity.AdjustedDeparture; // Varsa revize plan

            // Validasyonlar
            if (dto.RouteId.HasValue && dto.LineId.HasValue)
                await _tripTaskDomainService.ValidateRouteLineMatchAsync(dto.RouteId.Value, dto.LineId);

            if (dto.VehicleId.HasValue && dto.GarageId.HasValue)
                await _tripTaskDomainService.ValidateVehicleGarageMatchAsync(dto.VehicleId.Value, dto.GarageId);

            if (dto.VehicleId.HasValue && dto.VehicleId != oldVehicleId)
                await _tripTaskDomainService.ValidateVehicleAvailabilityAsync(dto.VehicleId.Value);

            var checkDriverId = dto.DriverId ?? entity.DriverId;
            var checkVehicleId = dto.VehicleId ?? entity.VehicleId;
            var checkStart = dto.ScheduledDeparture ?? entity.ScheduledDeparture;
            var checkEnd = dto.ScheduledArrival ?? entity.ScheduledArrival;
            var checkAdjStart = dto.AdjustedDeparture ?? entity.AdjustedDeparture;
            var checkAdjEnd = dto.AdjustedArrival ?? entity.AdjustedArrival;

            // Saat Validasyonları
            if (checkStart.HasValue && checkEnd.HasValue && checkEnd <= checkStart)
                throw new Exception("Planlanan varış saati, kalkış saatinden sonra olmalıdır.");

            if (checkAdjStart.HasValue && checkAdjEnd.HasValue && checkAdjEnd <= checkAdjStart)
                throw new Exception("Revize varış saati, revize kalkış saatinden sonra olmalıdır.");

            if (checkStart.HasValue && checkEnd.HasValue)
                await _tripTaskDomainService.ValidateAssignmentConflictAsync(
                    checkDriverId, checkVehicleId, checkStart.Value, checkEnd.Value, dto.Id);

            if (dto.DriverId.HasValue && (dto.DriverId != oldDriverId || dto.ScheduledDeparture != oldStart))
            {
                var taskDate = checkStart ?? DateTime.Now;
                await _driverDomainService.ValidateOperatorEligibilityAsync(dto.DriverId.Value, taskDate);
            }

            // --- DÜZELTME: userId burada tanımlandı, aşağıda tekrar tanımlanmayacak ---
            var userId = GetCurrentUserIdOrName();
            var user = await _userManager.FindByIdAsync(userId);
            bool isDriver = user != null && await _userManager.IsInRoleAsync(user, "Driver") &&
                            !await _userManager.IsInRoleAsync(user, "Chief") &&
                            !await _userManager.IsInRoleAsync(user, "Admin");

            _tripTaskDomainService.ValidateStatusTransition(entity.Status, dto.Status, isDriver);
            _tripTaskDomainService.ValidateStatusChange(dto.Status, dto.StatusReason);

            // --- MAPPING ---
            _mapper.Map(dto, entity);
            entity.UpdatedBy = userId;

            if (dto.Status == TaskState.Cancelled && oldStatus != TaskState.Cancelled)
            {
                if (entity.DriverId.HasValue)
                {
                    var driver = await _driverRepository.GetByIdWithDetailsAsync(entity.DriverId.Value);
                    if (driver != null)
                    {
                        driver.WorkStatus = WorkStatus.Available;
                        await _driverRepository.UpdateAsync(driver);
                    }
                }
                await SendCancellationEmailAsync(entity, dto.StatusReason);
            }

            await _repository.UpdateAsync(entity);
            await NotifyStateChange(entity.Id);

            // --- DEĞİŞİKLİK 2: GÜNCELLEME BİLDİRİM MANTIĞI ---
            if (dto.Status != TaskState.Cancelled && entity.DriverId.HasValue && entity.DriverId == oldDriverId)
            {
                bool isTimeChanged = false;

                // 1. Planlanan (Ana) saat değişti mi?
                if (dto.ScheduledDeparture.HasValue && dto.ScheduledDeparture != oldStart)
                    isTimeChanged = true;

                // 2. Revize (Adjusted) saat değişti mi?
                if (dto.AdjustedDeparture.HasValue && dto.AdjustedDeparture != oldAdjStart)
                    isTimeChanged = true;

                if (isTimeChanged || (dto.VehicleId.HasValue && dto.VehicleId != oldVehicleId))
                {
                    await SendUpdateNotificationAsync(entity, false);
                }
            }

            // --- ERKEN BAŞLAMA KONTROLÜ ---
            if (dto.Status == TaskState.InProgress && oldStatus == TaskState.Pending)
            {
                // Eğer Adjusted yoksa Scheduled'a bak, o da yoksa DateTime.Now al (Garanti olsun diye).
                // Böylece targetTime kesinlikle "DateTime" türünde olur ve .AddMinutes çalışır.
                DateTime targetTime = entity.AdjustedDeparture ?? entity.ScheduledDeparture ?? DateTime.Now;

                // Şoförün şu an bastığı zaman, Planlanan zamandan 5 dakika ve daha öncesi mi?
                if (DateTime.Now < targetTime.AddMinutes(-5))
                {

                    await _notificationService.SendNotificationAsync(
                        entity.CreatedBy, // Amir ID
                        "⚠️ Erken Başlama Uyarısı",
                        $"Şoför {entity.Driver?.User?.Name}, {targetTime:HH:mm} seferini çok erken ({DateTime.Now:HH:mm}) başlattı!",
                        "Info",
                        $"/Chief/TripTasks/Details/{entity.Id}"
                    );

                    _logger.LogWarning($"Erken Başlama: TaskId {entity.Id} planlanandan önce başlatıldı.");
                }
            }
        }

        public async Task DeleteAsync(Guid id, string? reason = null)
        {
            var entity = await _repository.GetByIdAsync(id);


            if (entity.DriverId.HasValue)
            {
                await SendCancellationEmailAsync(entity, reason ?? "Görev sistemden silindi.", isDelete: true);

                var driver = await _driverRepository.GetByIdWithDetailsAsync(entity.DriverId.Value);
                if (driver != null)
                {
                    driver.WorkStatus = WorkStatus.Available;
                    await _driverRepository.UpdateAsync(driver);
                }
            }

            await _repository.SoftDeleteAsync(entity, reason);
            await NotifyStateChange(id);
        }

        public async Task<ChiefDashboardDto> GetDashboardMetricsAsync(string? username)
        {
            var response = new ChiefDashboardDto();

            // 1. Tüm Görevleri Çek (Repository'deki mevcut GetAllAsync'i kullanıyoruz)
            //    Eğer username doluysa (Chief), sadece kendi görevlerini getirir.
            var allTasks = await _repository.GetAllAsync(username);

            // 2. Sadece BUGÜNÜN görevlerini filtrele
            var today = DateTime.Today;
            var todaysTasks = allTasks.Where(x => x.ScheduledDeparture.HasValue &&
                                                  x.ScheduledDeparture.Value.Date == today).ToList();

            // 3. KPI Hesapla
            response.TotalTasksToday = todaysTasks.Count;
            response.ActiveTrips = todaysTasks.Count(x => x.Status == TaskState.InProgress);
            response.CompletedToday = todaysTasks.Count(x => x.Status == TaskState.Completed);
            response.PendingIssues = todaysTasks.Count(x => x.Status == TaskState.Cancelled ||
                                                      x.Status == TaskState.Incomplete);

            // 4. Timeline Oluştur
            response.DailyTimeline = todaysTasks.OrderBy(x => x.ScheduledDeparture).Select(t =>
            {
                bool isDelayed = false;
                int delayMin = 0;

                if ((t.Status == TaskState.Pending || t.Status == TaskState.Accepted) &&
                    t.ScheduledDeparture < DateTime.Now)
                {
                    isDelayed = true;
                    delayMin = (int)(DateTime.Now - t.ScheduledDeparture.Value).TotalMinutes;
                }

                // Entity'den DTO'ya Map (Manuel Mapping)
                return new DashboardTimelineItemDto
                {
                    Id = t.Id,
                    LineCode = t.Line?.Code ?? "-",       // Entity Navigasyonu
                    RouteName = t.Route?.Name ?? "Belirsiz",
                    DriverName = t.Driver?.User != null ? $"{t.Driver.User.Name} {t.Driver.User.Surname}" : "Atanmadı",
                    PlateNumber = t.Vehicle?.PlateNumber ?? "-",
                    ScheduledTime = t.ScheduledDeparture ?? DateTime.Now,
                    ActualTime = t.ActualDeparture,
                    Status = t.Status,
                    IsDelayed = isDelayed,
                    DelayMinutes = delayMin
                };
            }).ToList();

            // ============================================================
            // 5. GENİŞLETİLMİŞ KRİTİK UYARILAR (İptaller + Gecikmeler)
            // ============================================================

            var allAlerts = new List<DashboardAlertDto>();

            // ------------------------------------------------------------
            // A) İPTAL VE YARIM KALANLAR (Yüksek Öncelik - Kırmızı/Danger)
            // ------------------------------------------------------------
            var criticalErrors = todaysTasks
                .Where(x => x.Status == TaskState.Cancelled || x.Status == TaskState.Incomplete)
                .Select(x => new DashboardAlertDto
                {
                    Id = x.Id,
                    Title = $"{x.Line?.Code ?? "-"} - İPTAL/ARIZA",
                    Reason = x.StatusReason ?? "Neden belirtilmedi",
                    Time = x.UpdatedAt ?? DateTime.Now,
                    AlertType = AlertType.Danger // View tarafında Kırmızı çerçeve için
                });

            allAlerts.AddRange(criticalErrors);

            // ------------------------------------------------------------
            // B) GECİKEN SEFERLER (Orta Öncelik - Turuncu/Warning)
            // Mantık: Statüsü hala 'Pending' veya 'Accepted' ise VE saati geçtiyse
            // ------------------------------------------------------------
            var delayedWarnings = todaysTasks
                .Where(x => (x.Status == TaskState.Pending || x.Status == TaskState.Accepted)
                            && x.ScheduledDeparture.HasValue
                            && x.ScheduledDeparture.Value < DateTime.Now) // Şimdiki zamanı geçmiş
                .Select(x => new DashboardAlertDto
                {
                    Id = x.Id,
                    Title = $"{x.Line?.Code ?? "-"} - GECİKME",
                    // Gecikme süresini hesaplayıp mesaj olarak yazıyoruz
                    Reason = $"Planlanan kalkış saati {(int)(DateTime.Now - x.ScheduledDeparture.Value).TotalMinutes} dakika geçti. Henüz çıkış yapılmadı.",
                    Time = DateTime.Now, // Olayın zamanı şu an
                    AlertType = AlertType.Warning // View tarafında Turuncu çerçeve için
                });

            allAlerts.AddRange(delayedWarnings);

            // ------------------------------------------------------------
            // C) LİSTEYİ BİRLEŞTİR, SIRALA VE DTO'YA EKLE
            // ------------------------------------------------------------
            response.CriticalAlerts = allAlerts
                .OrderByDescending(x => x.Time) // En son olan olay en üstte
                .Take(10) // Sayıyı biraz artırdık (Hem iptal hem gecikme sığsın)
                .ToList();

            // 6. Şoför İstatistikleri (IDriverRepository kullanıyoruz)
            var allDrivers = await _driverRepository.GetAllWithDetailsAsync(); // Tüm şoförleri çek
            if (allDrivers != null)
            {
                response.DriverStats = new DashboardDriverStatsDto
                {
                    TotalDrivers = allDrivers.Count(),
                    Active = allDrivers.Count(d => d.WorkStatus == WorkStatus.Working),
                    Available = allDrivers.Count(d => d.WorkStatus == WorkStatus.Available),
                    OnLeave = allDrivers.Count(d => d.WorkStatus == WorkStatus.OnVacation ||
                                                    d.WorkStatus == WorkStatus.MedicalLeave ||
                                                    d.WorkStatus == WorkStatus.AdministrativeLeave)
                };
            }

            return response;
        }

        // ============================================================
        // SÜRÜCÜ OPERASYON METOTLARI
        // ============================================================

        public async Task AcceptTripAsync(Guid taskId)
        {
            var task = await _repository.GetByIdAsync(taskId);
            if (task == null) throw new Exception("Görev bulunamadı.");

            if (task.Status != TaskState.Pending)
                throw new Exception("Sadece 'Beklemede' olan görevler kabul edilebilir.");

            task.Status = TaskState.Accepted;
            task.IsAcknowledged = true;
            task.AcknowledgedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(task);

            // 🔥 AMİRE BİLDİRİM (KABUL)
            var driverName = task.Driver?.User?.Name + " " + task.Driver?.User?.Surname;
            await NotifyChiefAsync(
                task,
                "✅ Görev Kabul Edildi",
                $"{driverName}, {task.Line?.Code} hattındaki görevi kabul etti.",
                "Info"
            );

            await NotifyStateChange(task.Id);
        }

        public async Task RejectTripAsync(Guid taskId, RejectTripRequestDto dto)
        {
            var task = await _repository.GetByIdAsync(taskId);
            if (task == null) throw new Exception("Görev bulunamadı.");

            if (task.Status != TaskState.Pending)
                throw new Exception("Sadece 'Beklemede' olan görevler reddedilebilir.");

            task.Status = TaskState.Cancelled;
            task.StatusReason = $"Şoför Reddetti: {dto.Reason}";

            if (task.DriverId.HasValue)
            {
                var driver = await _driverRepository.GetByIdWithDetailsAsync(task.DriverId.Value);
                if (driver != null)
                {
                    driver.WorkStatus = WorkStatus.Available;
                    await _driverRepository.UpdateAsync(driver);
                }
            }

            await _repository.UpdateAsync(task);

            // 🔥 AMİRE KRİTİK BİLDİRİM (RED)
            var driverName = task.Driver?.User?.Name + " " + task.Driver?.User?.Surname;
            await NotifyChiefAsync(
                task,
                "🚫 Görev Reddedildi!",
                $"{driverName}, {task.Line?.Code} hattındaki görevi reddetti. Neden: {dto.Reason}",
                "Warning" // Sarı/Turuncu İkon
            );

            await NotifyStateChange(task.Id);
        }

        public async Task StartTripAsync(Guid taskId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var task = await _repository.GetByIdAsync(taskId);
                if (task == null) throw new Exception("Görev bulunamadı.");

                // Şoförün devam eden başka görevi var mı kontrolü
                if (task.DriverId.HasValue)
                {
                    bool hasActiveTrip = await _context.TripTasks.AnyAsync(t =>
                        t.DriverId == task.DriverId &&
                        t.Status == TaskState.InProgress &&
                        t.Id != taskId &&
                        !t.IsDeleted);

                    if (hasActiveTrip) throw new Exception("Şu an devam eden başka bir seferiniz var.");
                }

                if (task.Status != TaskState.Accepted)
                    throw new Exception("Görevi başlatmak için önce 'Kabul Et'melisiniz.");

                task.Status = TaskState.InProgress;
                task.ActualDeparture = DateTime.Now;

                // Gecikme Hesabı (Çıkış Gecikmesi)
                if (task.ScheduledDeparture.HasValue)
                {
                    var targetTime = task.AdjustedDeparture ?? task.ScheduledDeparture.Value;
                    task.DelayOutMinutes = (int)(task.ActualDeparture.Value - targetTime).TotalMinutes;
                }

                // Araç Durumu
                if (task.VehicleId.HasValue)
                {
                    var vehicle = await _vehicleRepository.GetByIdAsync(task.VehicleId.Value);
                    if (vehicle != null)
                    {
                        task.StartOdometer = vehicle.TotalKm;
                        vehicle.ServiceStatus = ServiceStatus.OnRoute;
                        _context.Vehicles.Update(vehicle);
                    }
                }

                // Sürücü Durumu
                if (task.DriverId.HasValue)
                {
                    var driver = await _driverRepository.GetByIdWithDetailsAsync(task.DriverId.Value);
                    if (driver != null)
                    {
                        driver.WorkStatus = WorkStatus.Working;
                        await _driverRepository.UpdateAsync(driver);
                    }
                }

                // 🔥 AMİRE BİLDİRİM (BAŞLADI)
                // Sadece bilgi amaçlı, çok kritik değil ama takip için iyi.
                // Eğer çok fazla bildirim gidiyorsa bu kapatılabilir.
                var driverName = task.Driver?.User?.Name + " " + task.Driver?.User?.Surname;
                await NotifyChiefAsync(
                    task,
                    "🚌 Sefer Başladı",
                    $"{driverName}, {task.Line?.Code} hattındaki seferine başladı.",
                    "Info"
                );

                await _repository.UpdateAsync(task);
                await transaction.CommitAsync();
                await NotifyStateChange(task.Id);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task CompleteTripAsync(Guid taskId, CompleteTripRequestDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Entity'i 'Driver.User' ve 'Line' ile birlikte çekmek gerekebilir (isimler için)
                // Repository'de Include yoksa null gelebilir, o yüzden aşağıda kontrol edeceğiz.
                var task = await _repository.GetByIdAsync(taskId);
                if (task == null) throw new Exception("Görev bulunamadı.");

                if (task.Status != TaskState.InProgress)
                    throw new Exception("Sadece 'Devam Eden' görevler bitirilebilir.");

                task.Status = TaskState.Completed;
                task.PassengerCount = dto.PassengerCount;
                task.ActualArrival = DateTime.Now;

                if (!string.IsNullOrEmpty(dto.DriverNotes)) task.Description += $" | Not: {dto.DriverNotes}";

                if (task.ScheduledArrival.HasValue)
                {
                    var targetTime = task.AdjustedArrival ?? task.ScheduledArrival.Value;
                    task.DelayInMinutes = (int)(task.ActualArrival.Value - targetTime).TotalMinutes;
                }

                // Araç KM ve Yakıt İşlemleri
                if (task.VehicleId.HasValue)
                {
                    var vehicle = await _context.Vehicles.FindAsync(task.VehicleId.Value);
                    if (vehicle != null)
                    {
                        decimal startKm = task.StartOdometer ?? vehicle.TotalKm;
                        if (dto.EndOdometerInput < startKm)
                            throw new Exception($"Hata: Girdiğiniz KM ({dto.EndOdometerInput}), başlangıçtan ({startKm}) küçük olamaz.");

                        // Rota Sapma Kontrolü
                        if (task.Route != null)
                        {
                            decimal expectedDistKm = (decimal)task.Route.LengthInM / 1000m;
                            decimal actualDistKm = dto.EndOdometerInput - startKm;
                            decimal difference = Math.Abs(actualDistKm - expectedDistKm);

                            // %20 Sapma Varsa AMİRE UYARI AT
                            if (difference > (expectedDistKm * 0.2m))
                            {
                                task.Description += $" | UYARI: Rota sapması. Beklenen: {expectedDistKm:F1}km, Girilen: {actualDistKm:F1}km.";
                                await NotifyChiefAsync(task, "⚠️ KM Uyuşmazlığı", $"Seferde beklenen mesafeden %20 fazla/az sapma oldu. Kontrol ediniz.", "Warning");
                            }
                        }

                        task.EndOdometer = dto.EndOdometerInput;
                        vehicle.TotalKm = (int)dto.EndOdometerInput;
                        vehicle.ServiceStatus = ServiceStatus.InService;

                        if (dto.FuelLevel.HasValue)
                        {
                            vehicle.FuelLevel = dto.FuelLevel.Value;
                            task.Description += $" | Yakıt/Şarj: %{dto.FuelLevel}";

                            if (dto.FuelLevel.Value <= 15)
                            {
                                await NotifyChiefAsync(
                                    task,
                                    "⛽ Kritik Yakıt/Şarj",
                                    $"{task.Vehicle?.PlateNumber} plakalı araç seferden KRİTİK YAKIT (%{dto.FuelLevel}) seviyesiyle döndü. Dolum/Şarj gerekli.",
                                    "Warning"
                                );
                            }
                        }

                        _context.Vehicles.Update(vehicle);
                    }
                }

                // Şoförü Müsait Yap
                if (task.DriverId.HasValue)
                {
                    var driver = await _driverRepository.GetByIdWithDetailsAsync(task.DriverId.Value);
                    if (driver != null)
                    {
                        driver.WorkStatus = WorkStatus.Available;
                        await _driverRepository.UpdateAsync(driver);
                    }
                }

                // 🔥 YENİ EKLENEN: AMİRE BİLDİRİM (SEFER BİTTİ) 🔥
                var driverName = task.Driver?.User?.Name + " " + task.Driver?.User?.Surname;
                await NotifyChiefAsync(
                    task,
                    "✅ Sefer Tamamlandı",
                    $"{driverName}, {task.Line?.Code} hattındaki görevini başarıyla tamamladı.",
                    "Info"
                );

                // Gecikme Varsa Ekstra Uyarı
                int totalDelay = (task.DelayInMinutes ?? 0) + (task.DelayOutMinutes ?? 0);
                if (totalDelay > 15)
                {
                    await NotifyChiefAsync(
                        task,
                        "⚠️ Yüksek Gecikme",
                        $"{task.Line?.Code} hattında toplam {totalDelay} dakika gecikme ile sefer tamamlandı.",
                        "Warning"
                    );
                }

                await _repository.UpdateAsync(task);
                await transaction.CommitAsync();
                await NotifyStateChange(task.Id);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task FailTripAsync(Guid taskId, FailTripRequestDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var task = await _repository.GetByIdAsync(taskId);
                if (task == null) throw new Exception("Görev bulunamadı.");

                task.Status = TaskState.Incomplete;
                task.StatusReason = dto.Reason;
                task.ActualArrival = DateTime.Now;

                // Aracı Servis Dışı Yap
                if (task.VehicleId.HasValue)
                {
                    var vehicle = await _vehicleRepository.GetByIdAsync(task.VehicleId.Value);
                    if (vehicle != null)
                    {
                        vehicle.ServiceStatus = ServiceStatus.OutOfService;
                        vehicle.StatusReason = $"Seferde Arıza: {dto.Reason}";
                        _context.Vehicles.Update(vehicle);
                    }
                }

                // Şoförü Müsait Yap
                if (task.DriverId.HasValue)
                {
                    var driver = await _driverRepository.GetByIdWithDetailsAsync(task.DriverId.Value);
                    if (driver != null)
                    {
                        driver.WorkStatus = WorkStatus.Available;
                        await _driverRepository.UpdateAsync(driver);
                    }
                }

                // 🔥 YENİ EKLENEN: AMİRE KRİTİK BİLDİRİM 🔥
                var driverName = task.Driver?.User?.Name + " " + task.Driver?.User?.Surname;

                await NotifyChiefAsync(
                    task,
                    "🚨 ACİL: Sefer Yarım Kaldı!",
                    $"{driverName}, {task.Line?.Code} hattındaki seferi YARIM BIRAKTI. Neden: {dto.Reason}",
                    "Critical"
                );

                // Amir'e Mail Gönder (Opsiyonel ama iyi olur)
                if (!string.IsNullOrEmpty(task.CreatedBy) && task.CreatedBy != "System")
                {
                    var chiefUser = await _userManager.FindByIdAsync(task.CreatedBy);
                    if (chiefUser != null)
                    {
                        string emailBody = $"<h3>DİKKAT: Sefer Tamamlanamadı</h3><p><strong>Şoför:</strong> {driverName}</p><p><strong>Hat:</strong> {task.Line?.Code}</p><p><strong>Neden:</strong> {dto.Reason}</p>";
                        _ = Task.Run(async () => { try { await _emailService.SendEmailAsync(chiefUser.Email!, "🚨 ACİL: Sefer Arızası", emailBody, true); } catch { } });
                    }
                }

                await _repository.UpdateAsync(task);
                await transaction.CommitAsync();
                await NotifyStateChange(task.Id);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ============================================================
        // OKUMA & YARDIMCI METOTLAR
        // ============================================================

        public async Task<IEnumerable<TripTaskDto>> GetAllAsync(string? creatorName = null)
        {
            var allTasks = await _repository.GetAllAsync(creatorName);

            return _mapper.Map<IEnumerable<TripTaskDto>>(allTasks);
        }

        public async Task<TripTaskDto?> GetByIdAsync(Guid id) => _mapper.Map<TripTaskDto>(await _repository.GetByIdAsync(id));
        public async Task<IEnumerable<TripTaskDto>> GetByDriverIdAsync(Guid driverId) => _mapper.Map<IEnumerable<TripTaskDto>>(await _repository.GetByDriverIdAsync(driverId));
        public async Task<List<TripTaskDto>> SearchAsync(string query) => _mapper.Map<List<TripTaskDto>>(await _repository.SearchByTermAsync(query));

        public List<TaskState> GetAllowedStatesForRole(string role)
        {
            if (role == "Chief" || role == "Admin") return new List<TaskState> { TaskState.Pending, TaskState.Cancelled };
            if (role == "Driver") return new List<TaskState> { TaskState.Accepted, TaskState.InProgress, TaskState.Completed, TaskState.Incomplete };
            return new List<TaskState>();
        }

        // ============================================================
        // 🔔 BİLDİRİM VE MAIL YARDIMCI METOTLARI (GÜVENLİ VERSİYON)
        // ============================================================

        // 1. ŞOFÖRE BİLDİRİM GÖNDEREN MERKEZİ METOT
        private async Task NotifyDriverAsync(TripTask task, string title, string message, string type, bool isDelete = false)
        {
            // Şoför atalı değilse çık
            if (!task.DriverId.HasValue) return;

            string? userId = task.Driver?.User?.Id;

            // Eğer Driver.User yüklü değilse veritabanından çek
            if (string.IsNullOrEmpty(userId))
            {
                var driverEntity = await _driverRepository.GetByIdWithDetailsAsync(task.DriverId.Value);
                userId = driverEntity?.User?.Id;
            }

            // KULLANICI YOKSA HATA VERMEDEN ÇIK (Sistemi kilitleme)
            if (string.IsNullOrEmpty(userId)) return;

            // Link belirle
            string? linkUrl = isDelete ? null : $"/Driver/Tasks/Details/{task.Id}";

            try
            {
                await _notificationService.SendNotificationAsync(userId, title, message, type, linkUrl);
            }
            catch (Exception ex)
            {
                // Bildirim hatası yüzünden ana işlem (Sefer Başlat/Bitir) durmamalı.
                // Loglama yapılabilir: Console.WriteLine("Bildirim hatası: " + ex.Message);
            }
        }

        // 2. AMİRE BİLDİRİM GÖNDEREN MERKEZİ METOT
        private async Task NotifyChiefAsync(TripTask task, string title, string message, string type)
        {
            var targetUserIds = new List<string>();

            // A) ÖNCELİK: GÖREVİ OLUŞTURAN KİŞİYİ BUL
            if (!string.IsNullOrEmpty(task.CreatedBy) && task.CreatedBy != "System")
            {
                // 1. Senaryo: CreatedBy içinde direkt ID yazıyor olabilir.
                var userById = await _userManager.FindByIdAsync(task.CreatedBy);
                if (userById != null)
                {
                    targetUserIds.Add(userById.Id);
                }
                else
                {
                    // 2. Senaryo (Senin Durumun): CreatedBy içinde EMAIL veya KULLANICI ADI yazıyor.
                    var userByEmail = await _userManager.FindByEmailAsync(task.CreatedBy);
                    if (userByEmail != null)
                    {
                        targetUserIds.Add(userByEmail.Id); // 🔥 İŞTE BURADA EMAIL'İ ID'YE ÇEVİRİYORUZ
                    }
                    else
                    {
                        // Belki Username yazıyordur?
                        var userByName = await _userManager.FindByNameAsync(task.CreatedBy);
                        if (userByName != null)
                        {
                            targetUserIds.Add(userByName.Id);
                        }
                    }
                }
            }

            // B) YEDEK PLAN: Eğer oluşturan kişiyi bulamazsak, tüm "Chief" rolündekilere at
            if (!targetUserIds.Any())
            {
                var chiefs = await _userManager.GetUsersInRoleAsync("Chief");
                targetUserIds.AddRange(chiefs.Select(u => u.Id));
            }

            // Hiç kimse yoksa çık
            if (!targetUserIds.Any()) return;

            string linkUrl = $"/Chief/TripTasks/Details/{task.Id}";

            // Herkese Gönder
            foreach (var userId in targetUserIds.Distinct())
            {
                try
                {
                    // Artık elimizde kesinlikle geçerli bir GUID var. Hata vermez.
                    await _notificationService.SendNotificationAsync(userId, title, message, type, linkUrl);
                }
                catch
                {
                    // Loglama yapılabilir
                }
            }
        }

        // 3. YENİ GÖREV ATAMA
        private async Task SendAssignmentNotificationAsync(TripTask entity, TripTaskCreateDto dto)
        {
            // DİKKAT: GetByIdAsync metodunun Route ve Garage tablolarını Include ettiğinden emin ol!
            var fullTaskDetails = await _repository.GetByIdAsync(entity.Id);
            var driverUser = fullTaskDetails?.Driver?.User;

            if (fullTaskDetails != null && driverUser != null)
            {
                var placeholders = new Dictionary<string, string>
        {
            { "DriverName", $"{driverUser.Name} {driverUser.Surname}" },
            { "Date", dto.ScheduledDeparture.Value.ToString("dd.MM.yyyy") },
            { "TimeRange", $"{dto.ScheduledDeparture.Value:HH:mm} - {dto.ScheduledArrival.Value:HH:mm}" },
            
            // Hat Bilgileri
            { "LineCode", fullTaskDetails.Line?.Code ?? "X" },
            { "LineName", fullTaskDetails.Line?.Name ?? "-" },
            
            // --- EKSİK OLAN KISIMLAR EKLENDİ ---
            { "RouteName", fullTaskDetails.Route?.Name ?? "Standart Güzergah" },
            { "GarageName", fullTaskDetails.Garage?.GarageName ?? "Belirtilmedi" },
            // ------------------------------------

            { "PlateNumber", fullTaskDetails.Vehicle?.PlateNumber ?? "Atanmadı" },
            { "LinkUrl", "https://localhost:7060/Driver/Tasks" }
        };

                string emailBody = await _templateService.GenerateEmailBodyAsync("NewTaskAssignment.html", placeholders);

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _emailService.SendEmailAsync(driverUser.Email!, "Yeni Görev Ataması", emailBody, isHtml: true);
                    }
                    catch { }
                });

                await NotifyDriverAsync(entity, "Yeni Görev Ataması", $"Size {fullTaskDetails.Line?.Code} hattında görev atandı.", "TaskAssignment");
            }
        }

        // 4. GÖREV GÜNCELLEME
        private async Task SendUpdateNotificationAsync(TripTask entity, bool isCancelled)
        {
            string title = isCancelled ? "❌ Görev İPTAL Edildi" : "⚠️ Görev Güncellendi";
            string message = isCancelled
               ? $"Size atanan {entity.Line?.Code} hattındaki sefer iptal edilmiştir."
               : $"Size atanan {entity.Line?.Code} hattındaki seferin detayları güncellenmiştir.";

            string type = isCancelled ? "Warning" : "TaskUpdate";
            await NotifyDriverAsync(entity, title, message, type);
        }

        // 5. GÖREV İPTALİ / SİLME
        private async Task SendCancellationEmailAsync(TripTask task, string? reason, bool isDelete = false)
        {
            if (!task.DriverId.HasValue) return;

            var driverEntity = await _driverRepository.GetByIdWithDetailsAsync(task.DriverId.Value);
            var driverUser = driverEntity?.User;

            if (driverUser == null || string.IsNullOrEmpty(driverUser.Email)) return;

            var placeholders = new Dictionary<string, string>
             {
                 { "DriverName", $"{driverUser.Name} {driverUser.Surname}" },
                 { "LineCode", task.Line?.Code ?? "-" },
                 { "Date", task.ScheduledDeparture?.ToString("dd.MM.yyyy HH:mm") ?? "-" },
                 { "Reason", reason ?? "Operasyonel" }
             };

            string emailBody = await _templateService.GenerateEmailBodyAsync("TaskCancellation.html", placeholders);

            _ = Task.Run(async () => { try { await _emailService.SendEmailAsync(driverUser.Email, "❌ Görev İptali", emailBody, isHtml: true); } catch { } });

            await NotifyDriverAsync(task, "Görev İptal Edildi", $"Nedeni: {reason}", "Warning", isDelete);
        }

        // 6. CANLI GÜNCELLEME SİNYALİ
        private async Task NotifyStateChange(Guid taskId)
        {
            if (_hubContext != null)
            {
                await _hubContext.Clients.All.TaskUpdated(taskId);
            }
        }
    }
}