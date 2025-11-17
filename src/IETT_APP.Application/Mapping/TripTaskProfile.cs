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
            CreateMap<TripTask, TripTaskDto>()
                .ForMember(dest => dest.VehicleName, opt => opt.MapFrom(src => src.Vehicle != null ? src.Vehicle.PlateNumber : null))
                .ForMember(dest => dest.OperatorName, opt => opt.MapFrom(src => src.Operator != null ? src.Operator.Name : null))
                .ForMember(dest => dest.LineName, opt => opt.MapFrom(src => src.Line != null ? src.Line.Name : null))
                .ForMember(dest => dest.RouteName, opt => opt.MapFrom(src => src.Route != null ? src.Route.Name : null))
                .ForMember(dest => dest.GarageName, opt => opt.MapFrom(src => src.Garage != null ? src.Garage.GarageName : null))
                .ReverseMap(); // Eğer iki yönlü map gerekiyorsa

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
