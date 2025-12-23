using IETT_APP.Application.Dtos.Vehicle;
using IETT_APP.WebMVC.Areas.Planner.Models;

namespace IETT_APP.WebMVC.Areas.Planner.Extensions
{
    public static class VehicleMappingExtensions
    {
        // DTO → ViewModel
        public static VehicleViewModel ToViewModel(this VehicleDto<Guid> dto)
        {
            return new VehicleViewModel
            {
                Id = dto.Id,
                DoorNumber = dto.DoorNumber ?? string.Empty,
                PlateNumber = dto.PlateNumber ?? string.Empty,
                Capacity = dto.Capacity,
                GarageId = dto.GarageId,
                GarageName = string.Empty,
                ServiceStatus = dto.ServiceStatus,
                StatusReason = dto.StatusReason,
                Driver = dto.Driver,
                Model = dto.Model,
                Year = dto.Year,
                TotalKm = dto.TotalKm,
                HasDisabilityAccess = dto.HasDisabilityAccess,
                HasWiFi = dto.HasWiFi,
                HasBikeRack = dto.HasBikeRack,
                HasMobileCharging = dto.HasMobileCharging,
                HasPassengerInfoSystem = dto.HasPassengerInfoSystem,
                HasCCTV = dto.HasCCTV,
                IsActive = dto.IsActive,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt,
                IsDeleted = dto.IsDeleted
            };
        }

        // ViewModel → CreateDto
        public static VehicleCreateDto<Guid> ToCreateDto(this VehicleViewModel vm)
        {
            return new VehicleCreateDto<Guid>
            {
                DoorNumber = vm.DoorNumber,
                PlateNumber = vm.PlateNumber,
                Capacity = vm.Capacity,
                GarageId = vm.GarageId,
                ServiceStatus = vm.ServiceStatus,
                Driver = vm.Driver,
                Model = vm.Model,
                Year = vm.Year,
                TotalKm = vm.TotalKm,
                HasDisabilityAccess = vm.HasDisabilityAccess,
                HasWiFi = vm.HasWiFi,
                HasBikeRack = vm.HasBikeRack,
                HasMobileCharging = vm.HasMobileCharging,
                HasPassengerInfoSystem = vm.HasPassengerInfoSystem,
                HasCCTV = vm.HasCCTV,
                IsActive = vm.IsActive
            };
        }

        // ViewModel → UpdateDto
        public static VehicleUpdateDto<Guid> ToUpdateDto(this VehicleViewModel vm)
        {
            return new VehicleUpdateDto<Guid>
            {
                Id = vm.Id,
                DoorNumber = vm.DoorNumber,
                PlateNumber = vm.PlateNumber,
                Capacity = vm.Capacity,
                GarageId = vm.GarageId,
                ServiceStatus = vm.ServiceStatus,
                Driver = vm.Driver,
                Model = vm.Model,
                Year = vm.Year,
                TotalKm = vm.TotalKm,
                HasDisabilityAccess = vm.HasDisabilityAccess,
                HasWiFi = vm.HasWiFi,
                HasBikeRack = vm.HasBikeRack,
                HasMobileCharging = vm.HasMobileCharging,
                HasPassengerInfoSystem = vm.HasPassengerInfoSystem,
                HasCCTV = vm.HasCCTV,
                IsActive = vm.IsActive
            };
        }
    }
}
