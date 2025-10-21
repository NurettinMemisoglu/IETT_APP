using AutoMapper;
using IETT_APP.Application.Dtos.Route;
using IETT_APP.Domain.Entities;

namespace IETT_APP.Application.Mapping
{
    public class RouteProfile : Profile
    {
        public RouteProfile()
        {
            // Route <-> RouteDto
            CreateMap(typeof(Route<>), typeof(RouteDto<>)).ReverseMap();

            // LineCreateUpdateDto -> Route
            CreateMap(typeof(RouteCreateUpdateDto<>), typeof(Route<>))
                .ForMember("Id", opt => opt.Ignore())
                .ForMember("IsDeleted", opt => opt.Ignore())
                .ForMember("RouteStops", opt => opt.Ignore());

        }

    }
}
