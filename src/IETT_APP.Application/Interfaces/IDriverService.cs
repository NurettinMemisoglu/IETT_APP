using IETT_APP.Application.Dtos.Driver;
using Microsoft.AspNetCore.Http;

namespace IETT_APP.Application.Interfaces
{
    public interface IDriverService
    {
        // Okuma
        Task<IEnumerable<DriverDto>> GetAllAsync();
        Task<DriverDto?> GetByIdAsync(Guid id);
        Task<DriverDto?> GetByUserIdAsync(string userId);
        Task<IEnumerable<DriverDto>> GetUnassignedDriversAsync();

        // İşlemler
        Task<DriverDto> CreateAsync(CreateDriverDto dto); // Admin oluşturursa
        Task<DriverDto> CompleteProfileAsync(
            string userId,
            CompleteProfileDto dto,
            IFormFile? licenseDoc,
            IFormFile? psychoDoc); // Şoför kendisi doldurursa
        Task AssignGarageAsync(AssignGarageDto dto); // Admin garaj atarsa
        Task<string> UploadProfileImageAsync(Guid operatorId, UploadProfileImageDto dto);
        Task<DriverDto> UpdateProfileAsync(Guid driverId, UpdateDriverProfileDto dto);
        // Güncelleme & Silme
        Task UpdateAsync(UpdateDriverDto dto);
        Task DeleteAsync(Guid id);
    }
}