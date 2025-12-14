using AutoMapper;
using IETT_APP.Application.Dtos;
using IETT_APP.Domain.Entities;

namespace IETT_APP.Application.Mapping
{
    public class NotificationProfile : Profile
    {
        public NotificationProfile()
        {
            // Entity'den DTO'ya dönüşüm
            CreateMap<Notification, NotificationDto>();

            // Eğer DTO'dan Entity'ye dönüşüm gerekirse (örneğin bildirim oluşturma)
            // CreateMap<NotificationDto, Notification>(); 
        }
    }
}