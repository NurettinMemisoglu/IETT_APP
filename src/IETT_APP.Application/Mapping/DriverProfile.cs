using AutoMapper;
using IETT_APP.Application.Dtos.Driver;
using IETT_APP.Domain.Entities;

namespace IETT_APP.Application.Mapping
{
    public class DriverProfile : Profile
    {
        public DriverProfile()
        {
            // Entity -> DTO
            CreateMap<Driver, DriverDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.User.Name))
                .ForMember(dest => dest.Surname, opt => opt.MapFrom(src => src.User.Surname))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
                .ForMember(dest => dest.GarageName, opt => opt.MapFrom(src => src.Garage != null ? src.Garage.GarageName : null));

            // DTO -> Entity
            CreateMap<CreateDriverDto, Driver>();
            CreateMap<UpdateDriverDto, Driver>();

            // Complete Profile (Manuel mapleme de yapılabilir ama buraya ekleyelim)
            CreateMap<CompleteProfileDto, Driver>();

            // Şoförün kendi güncellemesi için mapping
            CreateMap<UpdateDriverProfileDto, Driver>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()) // ID değişmez
                .ForMember(dest => dest.User, opt => opt.Ignore()); // User nesnesini ezme
                                                                    // Diğer kritik alanları (Sicil, Ehliyet vb.) zaten DTO'da yok, 
                                                                    // AutoMapper sadece eşleşenleri günceller.

        }
    }
}