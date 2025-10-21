using AutoMapper;
using IETT_APP.Application.Dtos.Line;
using IETT_APP.Domain.Entities;

namespace IETT_APP.Application.Mapping
{
    public class LineProfile : Profile
    {
        public LineProfile()
        {
            // Line <-> LineDto
            CreateMap(typeof(Line<>), typeof(LineDto<>)).ReverseMap();

            // LineCreateUpdateDto -> Line
            CreateMap(typeof(LineCreateUpdateDto<>), typeof(Line<>))
                .ForMember("Id", opt => opt.Ignore())
                .ForMember("IsDeleted", opt => opt.Ignore());
        }
    }

}

