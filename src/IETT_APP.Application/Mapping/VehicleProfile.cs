using AutoMapper;
using IETT_APP.Application.Dtos.Vehicle;
using IETT_APP.Domain.Entities;

namespace IETT_APP.Application.Mapping
{
    public class VehicleProfile : Profile
    {
        public VehicleProfile()
        {
            // Vehicle <-> VehicleDto
            CreateMap(typeof(Vehicle<>), typeof(VehicleDto<>)).ReverseMap();

            // VehicleCreateDto -> Vehicle
            CreateMap(typeof(VehicleCreateDto<>), typeof(Vehicle<>))
                .ForMember("Id", opt => opt.Ignore())
                .ForMember("IsDeleted", opt => opt.Ignore());

            // VehicleUpdateDto -> Vehicle
            CreateMap(typeof(VehicleUpdateDto<>), typeof(Vehicle<>))
                .ForMember("IsDeleted", opt => opt.Ignore());
        }
    }
}
