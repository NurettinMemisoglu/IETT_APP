using IETT_APP.Application.Dtos.Route;
using IETT_APP.WebMVC.Areas.Planner.Models;

namespace IETT_APP.WebMVC.Extensions
{
    public static class RouteMappingExtensions
    {
        // DTO → ViewModel
        public static RouteViewModel ToViewModel(this RouteDto<Guid> route)
        {
            return new RouteViewModel
            {
                Id = route.Id,
                Code = route.Code ?? string.Empty,
                Name = route.Name ?? string.Empty,
                LengthInM = route.LengthInM,
                TimeInMinutes = route.TimeInMinutes,
                RoutesDirection = route.RoutesDirection,
                LineId = route.LineId,
                StopIds = route.StopIds ?? new List<Guid>(),
                StopNames = route.StopNames ?? new List<string>(),
                IsActive = route.IsActive,
                CreatedAt = route.CreatedAt,
                UpdatedAt = route.UpdatedAt
            };
        }

        // ViewModel → CreateUpdateDto
        public static RouteCreateUpdateDto<Guid> ToDto(this RouteViewModel vm)
        {
            return new RouteCreateUpdateDto<Guid>
            {
                Id = vm.Id,
                Code = vm.Code,
                Name = vm.Name,
                LengthInM = vm.LengthInM,
                TimeInMinutes = vm.TimeInMinutes,
                RoutesDirection = vm.RoutesDirection,
                LineId = vm.LineId,
                StopIds = vm.StopIds,
                StopNames = vm.StopNames,
                IsActive = vm.IsActive
            };
        }
    }
}
