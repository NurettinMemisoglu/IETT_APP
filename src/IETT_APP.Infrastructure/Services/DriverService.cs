using AutoMapper;
using IETT_APP.Application.Dtos.Driver; // Namespace güncellendi
using IETT_APP.Application.Interfaces;
using IETT_APP.Domain.Entities;
using IETT_APP.Domain.Enums;
using IETT_APP.Domain.Interfaces;
using IETT_APP.Infrastructure.Hubs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace IETT_APP.Infrastructure.Services
{
    public class DriverService : IDriverService
    {
        private readonly IDriverRepository _repository;
        private readonly ITripTaskRepository _tripTaskRepository;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly IFileService _fileService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHubContext<NotificationHub, INotificationClient> _hubContext;
        private readonly IMemoryCache _cache;

        public DriverService(
            IDriverRepository repository,
            ITripTaskRepository tripTaskRepository,
            UserManager<User> userManager,
            IMapper mapper,
            IFileService fileService,
            IHttpContextAccessor httpContextAccessor,
            IHubContext<NotificationHub, INotificationClient> hubContext,
            IMemoryCache cache)

        {
            _repository = repository;
            _tripTaskRepository = tripTaskRepository;
            _userManager = userManager;
            _mapper = mapper;
            _fileService = fileService;
            _httpContextAccessor = httpContextAccessor;
            _hubContext = hubContext;
            _cache = cache;

        }

        // Helper Metot: User ID'yi almak için
        private string GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "System";
        }

        // --- OKUMA İŞLEMLERİ ---
        public async Task<IEnumerable<DriverDto>> GetAllAsync()
        {
            var list = await _repository.GetAllWithDetailsAsync();
            return _mapper.Map<IEnumerable<DriverDto>>(list);
        }

        public async Task<DriverDto?> GetByIdAsync(Guid id)
        {
            var driver = await _repository.GetByIdWithDetailsAsync(id);
            return driver == null ? null : _mapper.Map<DriverDto>(driver);
        }

        public async Task<DriverDto?> GetByUserIdAsync(string userId)
        {
            var driver = await _repository.GetByUserIdAsync(userId);
            return driver == null ? null : _mapper.Map<DriverDto>(driver);
        }

        public async Task<IEnumerable<DriverDto>> GetUnassignedDriversAsync()
        {
            var list = await _repository.GetUnassignedDriversAsync();
            return _mapper.Map<IEnumerable<DriverDto>>(list);
        }

        // --- YAZMA İŞLEMLERİ ---

        // 1. Admin Manuel Oluşturursa
        public async Task<DriverDto> CreateAsync(CreateDriverDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null) throw new Exception("Kullanıcı bulunamadı.");

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                throw new Exception("Sistem yöneticileri (Admin) şoför yapılamaz.");
            }

            var existingDriver = await _repository.GetByUserIdAsync(dto.UserId);
            if (existingDriver != null) throw new Exception("Bu kullanıcının zaten şoför kaydı mevcut.");

            // Rol Ataması
            if (!await _userManager.IsInRoleAsync(user, "Driver"))
                await _userManager.AddToRoleAsync(user, "Driver");

            var entity = _mapper.Map<Driver>(dto);
            entity.Id = Guid.NewGuid();
            entity.WorkStatus = WorkStatus.OffDuty;
            entity.IsActive = true;

            await _repository.AddAsync(entity);
            return _mapper.Map<DriverDto>(entity);
        }

        public async Task<bool> IsProfileCompleteAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return false;

            string cacheKey = $"profile_complete_{userId}";

            // Önce hafızaya bak, orada varsa direkt döndür (Veritabanına gitme)
            if (_cache.TryGetValue(cacheKey, out bool isComplete))
            {
                return isComplete;
            }

            // Hafızada yoksa veritabanına sor
            var driver = await _repository.GetByUserIdAsync(userId);
            isComplete = (driver != null);

            // Sonucu hafızaya yaz (Örneğin 30 dakika boyunca hatırla)
            // Böylece 30 dk boyunca tekrar veritabanına sormaz.
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(30));

            _cache.Set(cacheKey, isComplete, cacheOptions);

            return isComplete;
        }

        // 2. Şoför Kendi Profilini Tamamlarsa (ONBOARDING)
        public async Task<DriverDto> CompleteProfileAsync(
            string userId,
            CompleteProfileDto dto,
            IFormFile? licenseDoc,
            IFormFile? psychoDoc)
        {
            // 1. Validasyonlar (Kullanıcı, Rol, Mevcut Profil vb.)
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new Exception("Kullanıcı bulunamadı.");

            if (await _userManager.IsInRoleAsync(user, "Admin"))
                throw new Exception("Admin hesabı ile şoför profili oluşturulamaz.");

            var existingDriver = await _repository.GetByUserIdAsync(userId);
            if (existingDriver != null) throw new Exception("Profiliniz zaten oluşturulmuş.");

            if (!await _userManager.IsInRoleAsync(user, "Driver"))
                await _userManager.AddToRoleAsync(user, "Driver");

            // 2. Entity Oluşturma (DTO'dan gelen metin verileri)
            var entity = _mapper.Map<Driver>(dto);
            entity.Id = Guid.NewGuid();
            entity.UserId = userId;
            entity.WorkStatus = WorkStatus.OffDuty;
            entity.IsActive = true;
            entity.DriverType = DriverType.IETT_Staff;
            if (entity.EmploymentDate == default) entity.EmploymentDate = DateTime.Today;

            // 3. DOSYA KAYDETME VE YOL ATAMA
            // DTO yerine parametreden gelen dosyaları kullanıyoruz.

            // A) Ehliyet Belgesi
            if (licenseDoc != null && licenseDoc.Length > 0)
            {
                var savedFile = await _fileService.SaveFileAsync(licenseDoc, FileCategory.Document, userId);
                entity.LicenseDocumentPath = savedFile.FilePath;
            }

            // B) Psikoteknik Belgesi
            if (psychoDoc != null && psychoDoc.Length > 0)
            {
                var savedFile = await _fileService.SaveFileAsync(psychoDoc, FileCategory.Document, userId);
                entity.PsychotechnicDocumentPath = savedFile.FilePath;
            }

            try
            {
                // Kayıt işlemi
                await _repository.AddAsync(entity);
                _cache.Remove($"profile_complete_{userId}");
                return _mapper.Map<DriverDto>(entity);
            }
            catch (Exception ex)
            {
                // 🔥 SİHİRLİ DOKUNUŞ: Inner Exception'ı yakala
                // Hata mesajının en derinindeki sebebi buluyoruz
                var errorMessage = ex.InnerException != null
                    ? ex.InnerException.Message
                    : ex.Message;

                throw new Exception($"Veritabanı Kayıt Hatası: {errorMessage}");
            }
        }

        // 3. Admin Garaj Atarsa
        public async Task AssignGarageAsync(AssignGarageDto dto)
        {
            var driverEntity = await _repository.GetByIdWithDetailsAsync(dto.DriverId); // DTO'da DriverId olmalı
            if (driverEntity == null) throw new Exception("Sürücü bulunamadı.");

            driverEntity.GarageId = dto.GarageId;

            if (driverEntity.WorkStatus == WorkStatus.OffDuty)
            {
                driverEntity.WorkStatus = WorkStatus.Available;
            }

            await _repository.UpdateAsync(driverEntity);
        }

        // 4. Profil Resmi Yükleme
        public async Task<string> UploadProfileImageAsync(Guid driverId, UploadProfileImageDto dto)
        {
            var driverEntity = await _repository.GetByIdWithDetailsAsync(driverId);
            if (driverEntity == null) throw new Exception("Sürücü bulunamadı.");

            // 1. Eski Resim Yolunu Sakla
            string? oldFilePath = driverEntity.ProfileImagePath;

            // 2. Yeni Resmi Kaydet
            string currentUserId = GetCurrentUserId(); // BaseService'den geliyorsa
            var savedFile = await _fileService.SaveFileAsync(dto.Photo, FileCategory.ProfileImage, currentUserId);

            // 3. Veritabanını Güncelle
            driverEntity.ProfileImagePath = savedFile.FilePath;
            await _repository.UpdateAsync(driverEntity);

            // 4. Eski Resmi Sil
            if (!string.IsNullOrEmpty(oldFilePath) && oldFilePath != savedFile.FilePath)
            {
                await _fileService.DeleteFileAsync(oldFilePath);
            }

            // ========================================================================
            // 🚀 KRİTİK EKLEME: SIGNALR İLE BİLDİRİM GÖNDER
            // ========================================================================
            if (!string.IsNullOrEmpty(driverEntity.UserId))
            {
                // Kullanıcıya anlık haber ver: "Resmin değişti, hemen yenile!"
                await _hubContext.Clients.User(driverEntity.UserId)
                                 .ProfileImageUpdated(driverEntity.ProfileImagePath);
            }
            // ========================================================================

            return driverEntity.ProfileImagePath;
        }

        public async Task UpdateAsync(UpdateDriverDto dto)
        {
            var entity = await _repository.GetByIdWithDetailsAsync(dto.Id);
            if (entity == null) throw new Exception("Sürücü bulunamadı.");

            _mapper.Map(dto, entity);
            await _repository.UpdateAsync(entity);
        }

        // DriverService.cs

        public async Task<DriverDto> UpdateProfileAsync(Guid driverId, UpdateDriverProfileDto dto)
        {
            // DTO'dan değil, parametreden gelen güvenli ID'yi kullanıyoruz
            var entity = await _repository.GetByIdWithDetailsAsync(driverId);

            if (entity == null) throw new Exception("Sürücü bulunamadı.");

            // 1. Telefon Numarasını Güncelle (User tablosunda)
            // Null check eklemek iyi olur, telefon silinmesin.
            if (!string.IsNullOrEmpty(dto.PhoneNumber) && entity.User.PhoneNumber != dto.PhoneNumber)
            {
                entity.User.PhoneNumber = dto.PhoneNumber;
                await _userManager.UpdateAsync(entity.User);
            }

            // 2. Diğer Alanları Güncelle (Driver tablosunda)
            // Manuel set ederek sadece izin verilen alanları değiştiriyoruz.
            entity.Address = dto.Address;
            entity.EmergencyContactName = dto.EmergencyContactName;
            entity.EmergencyContactPhone = dto.EmergencyContactPhone;
            entity.BloodType = dto.BloodType;
            entity.HasChronicDisease = dto.HasChronicDisease;
            entity.HealthNotes = dto.HealthNotes;

            // 3. Kaydet
            await _repository.UpdateAsync(entity);

            // 4. Güncel DTO dön
            return _mapper.Map<DriverDto>(entity);
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _repository.GetByIdWithDetailsAsync(id);
            if (entity == null) throw new Exception("Sürücü bulunamadı.");

            await _repository.SoftDeleteAsync(entity);
        }

        // ============================================================
        // 🚀 DRIVER DASHBOARD & UI LOGIC (GÜNCELLENDİ - V2)
        // ============================================================
        public async Task<DriverDashboardDto> GetDriverDashboardAsync(string userId)
        {
            // 1. Şoför Bilgisi
            var driver = await _repository.GetByUserIdAsync(userId);
            if (driver == null) throw new Exception("Sürücü profili bulunamadı.");

            // 2. Tüm Görevleri Çek
            var allTasks = await _tripTaskRepository.GetByDriverIdAsync(driver.Id);

            // 3. FİLTRELEME VE SIRALAMA (REVİZE EDİLDİ)
            var today = DateTime.Today;

            var relevantTasks = allTasks.Where(t =>
                t.Status == TaskState.InProgress ||
                t.Status == TaskState.Accepted ||
                (t.Status == TaskState.Pending && t.ScheduledDeparture >= today) || // Geçmişteki "Bekleyen"leri getirme
                (t.Status == TaskState.Completed && t.ScheduledDeparture?.Date == today)
            )
            // KRİTİK NOKTA: Sıralamayı yaparken Revize Saat (Adjusted) varsa onu kullan, yoksa Normal Saati kullan.
            .OrderBy(t => t.AdjustedDeparture ?? t.ScheduledDeparture)
            .ToList();

            // 4. Ana DTO'yu Oluştur
            var dashboard = new DriverDashboardDto
            {
                FullName = $"{driver.User?.Name} {driver.User?.Surname}",
                ProfileImageUrl = driver.ProfileImagePath ?? string.Empty,
                WorkStatus = driver.WorkStatus,
                CompletedTasksCount = relevantTasks.Count(t => t.Status == TaskState.Completed)
            };

            // =========================================================================
            // 5. AKTİF GÖREVİ BELİRLE (BÜYÜK KART İÇİN YENİ MANTIK)
            // =========================================================================

            TripTask? activeTaskEntity = null;

            // KURAL 1: Eğer şoför şu an sürüşteyse (InProgress), ekranda kesinlikle o görünmeli.
            activeTaskEntity = relevantTasks.FirstOrDefault(t => t.Status == TaskState.InProgress);

            // KURAL 2: Eğer sürüşte değilse, ZAMAN OLARAK EN YAKIN / İLK görevi getir.
            // Statüsünün "Kabul Edildi" veya "Bekliyor" olması fark etmez.
            // Listeyi zaten yukarıda .OrderBy ile zamana göre sıraladığımız için listenin başındaki eleman en yakındır.
            if (activeTaskEntity == null)
            {
                activeTaskEntity = relevantTasks.FirstOrDefault(t =>
                    t.Status == TaskState.Accepted ||
                    t.Status == TaskState.Pending);
            }

            // Bulunan görevi DTO'ya map'le
            if (activeTaskEntity != null)
            {
                dashboard.CurrentTask = _mapper.Map<DashboardTaskDto>(activeTaskEntity);
            }

            // 6. SIRADAKİ GÖREVLER (ALT LİSTE - 7 GÜNLÜK FİLTRE)
            var oneWeekLater = DateTime.Today.AddDays(7); // 1 Hafta sonrası

            var upcomingEntities = relevantTasks
                .Where(t => t.Id != activeTaskEntity?.Id && // Aktif olan hariç
                            t.Status != TaskState.Completed &&
                            t.Status != TaskState.Cancelled &&
                            t.Status != TaskState.Incomplete &&
                            // YENİ: Sadece önümüzdeki 7 gün
                            (t.AdjustedDeparture ?? t.ScheduledDeparture) < oneWeekLater)
                .OrderBy(t => t.AdjustedDeparture ?? t.ScheduledDeparture)
                .ToList();

            dashboard.UpcomingTasks = _mapper.Map<List<DashboardTaskDto>>(upcomingEntities);

            return dashboard;
        }
    }
}
