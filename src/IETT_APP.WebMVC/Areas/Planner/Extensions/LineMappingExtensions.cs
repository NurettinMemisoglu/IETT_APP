using IETT_APP.Application.Dtos.Line;
using IETT_APP.WebMVC.Areas.Planner.Models;

namespace IETT_APP.WebMVC.Areas.Planner.Extensions
{
    public static class LineMappingExtensions
    {
        // Map from generic LineDto<Guid> to LineViewModel
        public static LineViewModel ToViewModel(this LineDto<Guid> line)
        {
            return new LineViewModel
            {
                Id = line.Id,
                Code = line.Code ?? string.Empty,
                Name = line.Name ?? string.Empty,
                // If your DTO includes Description/CreatedAt/UpdatedAt, map them; otherwise defaults remain
                Description = (line as dynamic)?.Description,
                CreatedAt = (line as dynamic)?.CreatedAt ?? default,
                UpdatedAt = (line as dynamic)?.UpdatedAt,
                IsActive = line.IsActive,
                VehicleCount = line.VehicleCount,
                LineType = line.LineType
            };
        }
    }
}
