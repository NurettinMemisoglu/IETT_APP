using IETT_APP.Application.Dtos;

namespace IETT_APP.WebMVC.Services.Interfaces
{
    public interface INotificationApiService
    {
        // Sadece okunmamışları getirir (Zil için)
        Task<IEnumerable<NotificationDto>> GetUnreadNotificationsAsync();

        // 🔥 YENİ EKLENEN: Tüm bildirim geçmişini getirir (Sayfa için)
        Task<IEnumerable<NotificationDto>> GetAllAsync();

        // Okundu işaretler
        Task MarkAsReadAsync(Guid id);
    }
}