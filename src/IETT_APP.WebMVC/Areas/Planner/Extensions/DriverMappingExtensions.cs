// IETT_APP.WebUI/Extensions/MappingExtensions.cs (Mevcut dosyan varsa içine ekle)

using IETT_APP.Application.Dtos.Driver;
using IETT_APP.WebMVC.Areas.Planner.Models;

public static partial class DriverMappingExtensions
{
    public static AssignGarageDto ToDto(this AssignGarageViewModel model)
    {
        return new AssignGarageDto
        {
            DriverId = model.DriverId,
            GarageId = model.GarageId
        };
    }
}