using AutoMapper;
using IETT_APP.Application.Dtos.Driver; // Namespace güncellendi
using IETT_APP.Application.Interfaces;
using IETT_APP.Domain.Entities;
using IETT_APP.Domain.Enums;
using IETT_APP.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace IETT_APP.Infrastructure.Services
{
    public class DriverService : IDriverService
    {
        private readonly IDriverRepository _repository;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly IFileService _fileService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DriverService(
            IDriverRepository repository,
            UserManager<User> userManager,
            IMapper mapper,
            IFileService fileService,
            IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _userManager = userManager;
            _mapper = mapper;
            _fileService = fileService;
            _httpContextAccessor = httpContextAccessor;
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
            string currentUserId = GetCurrentUserId();
            var savedFile = await _fileService.SaveFileAsync(dto.Photo, FileCategory.ProfileImage, currentUserId);

            // 3. Veritabanını Güncelle
            driverEntity.ProfileImagePath = savedFile.FilePath;
            await _repository.UpdateAsync(driverEntity);

            // 4. Eski Resmi ve DB Kaydını Sil (GÜNCELLENDİ)
            if (!string.IsNullOrEmpty(oldFilePath))
            {
                if (oldFilePath != savedFile.FilePath)
                {
                    // ARTIK AWAIT İLE ÇAĞIRIYORUZ
                    await _fileService.DeleteFileAsync(oldFilePath);
                }
            }

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
    }
}