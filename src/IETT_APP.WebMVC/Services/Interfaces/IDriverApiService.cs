using IETT_APP.Application.Dtos.Driver;
using IETT_APP.Application.Wrappers; // ServiceResult

namespace IETT_APP.WebMVC.Services.Interfaces
{
    public interface IDriverApiService
    {
        // Okuma İşlemleri
        Task<IEnumerable<DriverDto>> GetAllAsync();
        Task<DriverDto?> GetByIdAsync(Guid id);
        Task<DriverDto?> GetByUserIdAsync(string userId);
        Task<IEnumerable<DriverDto>> GetUnassignedDriversAsync();

        // Yazma İşlemleri
        Task<ServiceResult<DriverDto>> CreateAsync(CreateDriverDto dto);
        Task<ServiceResult> UpdateAsync(UpdateDriverDto dto);
        Task<ServiceResult> DeleteAsync(Guid id);
        Task<ServiceResult> UpdateProfileAsync(UpdateDriverProfileDto dto);

        // Özel İşlemler
        Task<ServiceResult> AssignGarageAsync(AssignGarageDto dto);
        Task<ServiceResult<DriverDto>> CompleteProfileAsync(string userId, CompleteProfileDto dto);

        // Dosya Yükleme (Özel İşlem)

        // DTO yerine direkt IFormFile kullanıyoruz
        Task<ServiceResult<string>> UploadProfileImageAsync(Guid driverId, IFormFile photo);
        Task<DriverDashboardDto?> GetDashboardAsync();
    }
}