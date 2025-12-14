using IETT_APP.Application.Dtos.Driver;
using IETT_APP.Application.Dtos.TripTask; // Eklendi
using IETT_APP.WebMVC.Areas.Driver.Models;

namespace IETT_APP.WebMVC.Areas.Driver.Extensions
{
    public static class DriverMappingExtensions
    {
        // 1. DTO -> ViewModel (Profil Sayfası)
        public static DriverProfileViewModel ToViewModel(this DriverDto dto)
        {
            return new DriverProfileViewModel
            {
                Id = dto.Id,
                UserId = dto.UserId,
                FullName = $"{dto.Name} {dto.Surname}",
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                EmployeeNumber = dto.EmployeeNumber,
                GarageName = dto.GarageName ?? "Atanmadı",
                WorkStatus = dto.WorkStatus,
                ProfileImagePath = dto.ProfileImagePath,

                Address = dto.Address,
                EmergencyContactName = dto.EmergencyContactName,
                EmergencyContactPhone = dto.EmergencyContactPhone,

                BloodType = dto.BloodType,
                HasChronicDisease = dto.HasChronicDisease,
                HealthNotes = dto.HealthNotes,
                SrcCertificateNumber = dto.SrcCertificateNumber,

                LicenseNumber = dto.LicenseNumber,
                LicenseClass = dto.LicenseClass,
                LicenseExpiryDate = dto.LicenseExpiryDate,
                PsychotechnicExpiryDate = dto.PsychotechnicExpiryDate,
                LicenseDocumentPath = dto.LicenseDocumentPath,
                PsychotechnicDocumentPath = dto.PsychotechnicDocumentPath,
            };
        }

        // 2. ViewModel -> UpdateDriverProfileDto (Profil Güncelleme)
        public static UpdateDriverProfileDto ToUpdateProfileDto(this DriverProfileViewModel vm)
        {
            return new UpdateDriverProfileDto
            {
                PhoneNumber = vm.PhoneNumber,
                Address = vm.Address,
                EmergencyContactName = vm.EmergencyContactName,
                EmergencyContactPhone = vm.EmergencyContactPhone,
                BloodType = vm.BloodType,
                HasChronicDisease = vm.HasChronicDisease,
                HealthNotes = vm.HealthNotes
            };
        }

        // 3. 🔥 YENİ EKLENEN: TripTaskDto -> DriverTripTaskViewModel (Görevlerim Sayfası)
        public static DriverTripTaskViewModel ToDriverViewModel(this TripTaskDto dto)
        {
            return new DriverTripTaskViewModel
            {
                Id = dto.Id,
                Title = dto.Title,
                Description = dto.Description,
                Status = dto.Status,

                // Zamanlama
                ScheduledDeparture = dto.ScheduledDeparture,
                ScheduledArrival = dto.ScheduledArrival,
                AdjustedDeparture = dto.AdjustedDeparture,
                ActualDeparture = dto.ActualDeparture,
                ActualArrival = dto.ActualArrival,

                // Kaynak İsimleri (Null kontrolü ile)
                VehicleName = dto.VehicleName ?? "Atanmadı",
                LineName = dto.LineName ?? "-",
                RouteName = dto.RouteName ?? "-",
                GarageName = dto.GarageName ?? "-"

                // Input alanları (PassengerCount vb.) boş kalır, çünkü onlar formdan gelecek.
            };
        }
    }
}