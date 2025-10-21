using IETT_APP.Application.Dtos.Stop;
using IETT_APP.WebMVC.Areas.Planner.Models;

namespace IETT_APP.WebMVC.Areas.Planner.Extensions
{
    public static class StopMappingExtensions
    {
        // StopViewModel -> CreateStopDto
        public static CreateStopDto ToCreateDto(this StopViewModel vm)
        {
            return new CreateStopDto
            {
                Code = vm.Code,
                Name = vm.Name,
                District = vm.District,
                StopType = vm.StopType,
                SmartStop = vm.SmartStop,
                Location = new LocationDto
                {
                    Latitude = vm.Location.Latitude,
                    Longitude = vm.Location.Longitude
                }
            };
        }

        // StopViewModel -> UpdateStopDto
        public static UpdateStopDto ToUpdateDto(this StopViewModel vm)
        {
            return new UpdateStopDto
            {
                Code = vm.Code,
                Name = vm.Name,
                District = vm.District,
                StopType = vm.StopType,
                SmartStop = vm.SmartStop,
                Location = new LocationDto
                {
                    Latitude = vm.Location.Latitude,
                    Longitude = vm.Location.Longitude
                }
            };
        }

        // StopDto -> StopViewModel
        public static StopViewModel ToViewModel(this StopDto dto)
        {
            return new StopViewModel
            {
                Id = dto.Id,
                Code = dto.Code,
                Name = dto.Name,
                District = dto.District,
                StopType = dto.StopType,
                SmartStop = dto.SmartStop,
                Location = new LocationViewModel
                {
                    Latitude = dto.Location.Latitude,
                    Longitude = dto.Location.Longitude
                }
            };
        }
    }
}
