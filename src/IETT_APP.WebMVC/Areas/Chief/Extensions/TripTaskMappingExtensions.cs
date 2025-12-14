using IETT_APP.Application.Dtos.TripTask;
using IETT_APP.WebMVC.Areas.Chief.Models;

namespace IETT_APP.WebMVC.Areas.Chief.Extensions
{
    public static class TripTaskMappingExtensions
    {
        // === DTO → ViewModel ===
        public static TripTaskViewModel ToViewModel(this TripTaskDto dto)
        {
            return new TripTaskViewModel
            {
                Id = dto.Id,
                Title = dto.Title ?? string.Empty,
                Description = dto.Description ?? string.Empty,
                Status = dto.Status,
                StatusReason = dto.StatusReason,
                PassengerCount = dto.PassengerCount,
                DelayInMinutes = dto.DelayInMinutes,
                DelayOutMinutes = dto.DelayOutMinutes,
                ScheduledDeparture = dto.ScheduledDeparture,
                ScheduledArrival = dto.ScheduledArrival,
                AdjustedDeparture = dto.AdjustedDeparture,
                AdjustedArrival = dto.AdjustedArrival,
                ActualDeparture = dto.ActualDeparture,
                ActualArrival = dto.ActualArrival,
                VehicleId = dto.VehicleId,
                DriverId = dto.DriverId,
                LineId = dto.LineId,
                RouteId = dto.RouteId,
                GarageId = dto.GarageId,
                CreatedAt = dto.CreatedAt ?? System.DateTime.UtcNow,
                UpdatedAt = dto.UpdatedAt,
                IsDeleted = dto.IsDeleted,

                // Name alanları DTO’dan dolacak
                VehicleName = dto.VehicleName ?? string.Empty,
                DriverName = dto.DriverName ?? string.Empty,
                LineName = dto.LineName ?? string.Empty,
                RouteName = dto.RouteName ?? string.Empty,
                GarageName = dto.GarageName ?? string.Empty
            };
        }

        // === ViewModel → CreateDto ===
        public static TripTaskCreateDto ToCreateDto(this TripTaskViewModel vm)
        {
            return new TripTaskCreateDto
            {
                Title = vm.Title,
                Description = vm.Description,
                ScheduledDeparture = vm.ScheduledDeparture,
                ScheduledArrival = vm.ScheduledArrival,
                VehicleId = vm.VehicleId,
                DriverId = vm.DriverId,
                LineId = vm.LineId,
                RouteId = vm.RouteId,
                GarageId = vm.GarageId
            };
        }

        // === ViewModel → UpdateDto ===
        public static TripTaskUpdateDto ToUpdateDto(this TripTaskViewModel vm)
        {
            return new TripTaskUpdateDto
            {
                Id = vm.Id,
                Title = vm.Title,
                Description = vm.Description,
                Status = vm.Status,
                StatusReason = vm.StatusReason,
                PassengerCount = vm.PassengerCount,
                DelayInMinutes = vm.DelayInMinutes,
                DelayOutMinutes = vm.DelayOutMinutes,
                ScheduledDeparture = vm.ScheduledDeparture,
                ScheduledArrival = vm.ScheduledArrival,
                AdjustedDeparture = vm.AdjustedDeparture,
                AdjustedArrival = vm.AdjustedArrival,
                ActualDeparture = vm.ActualDeparture,
                ActualArrival = vm.ActualArrival,
                VehicleId = vm.VehicleId,
                DriverId = vm.DriverId,
                LineId = vm.LineId,
                RouteId = vm.RouteId,
                GarageId = vm.GarageId
            };
        }
    }
}
