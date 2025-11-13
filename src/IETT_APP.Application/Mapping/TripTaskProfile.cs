using AutoMapper;
using IETT_APP.Application.Dtos.TripTask;
using IETT_APP.Domain.Entities;

namespace IETT_APP.Application.Mapping
{
    public class TripTaskProfile : Profile
    {
        public TripTaskProfile()
        {
            // TripTask <-> TripTaskDto
            CreateMap<TripTask, TripTaskDto>().ReverseMap();

            // TripTaskCreateDto -> TripTask
            CreateMap<TripTaskCreateDto, TripTask>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.TripTaskHistories, opt => opt.Ignore());

            // TripTaskUpdateDto -> TripTask
            CreateMap<TripTaskUpdateDto, TripTask>()
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.TripTaskHistories, opt => opt.Ignore());
        }
    }
}
