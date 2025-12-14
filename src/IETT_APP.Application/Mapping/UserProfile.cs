using AutoMapper;
using IETT_APP.Application.Dtos;
using IETT_APP.Domain.Entities;

namespace IETT_APP.Application.Mapping
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            // Entity -> DTO
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.RoleNames, opt => opt.Ignore()); // Service katmanında doldurulacak

            // DTO -> Entity (Create/Update işlemleri için)
            CreateMap<UserDto, User>()
                .ForMember(dest => dest.RoleNames, opt => opt.Ignore()) // Entity'de RoleNames yok
                                                                        // Aşağıdaki alanlar Repository'de veya DB tarafında yönetilir, DTO'dan gelmemeli:
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedBy, opt => opt.Ignore());
        }
    }
}