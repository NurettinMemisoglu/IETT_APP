using IETT_APP.Application.Dtos.Line;
using IETT_APP.WebMVC.Areas.Planner.Models;

public static class LineMappingExtensions
{
    // Mevcut: DTO → ViewModel
    public static LineViewModel ToViewModel(this LineDto<Guid> line)
    {
        return new LineViewModel
        {
            Id = line.Id,
            Code = line.Code ?? string.Empty,
            Name = line.Name ?? string.Empty,
            LineType = line.LineType,
            VehicleCount = line.VehicleCount,
            IsActive = line.IsActive,
            CreatedAt = line.CreatedAt,
            UpdatedAt = line.UpdatedAt
        };
    }

    // Yeni: ViewModel → LineCreateUpdateDto<Guid>
    public static LineCreateUpdateDto<Guid> ToDto(this LineViewModel vm)
    {
        return new LineCreateUpdateDto<Guid>
        {
            Id = vm.Id,
            Code = vm.Code,
            Name = vm.Name,
            LineType = vm.LineType,
            VehicleCount = vm.VehicleCount,
            IsActive = vm.IsActive
        };
    }
}
