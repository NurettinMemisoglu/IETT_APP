using IETT_APP.Application.Dtos;

namespace IETT_APP.Application.Interfaces
{
    public interface INotificationService
    {
        // DÜZELTME: Sınıfındaki isimle (SendNotificationAsync) aynı olmalı
        Task SendNotificationAsync(string userId, string title, string message, string type = "Info", string? linkUrl = null);

        Task<IEnumerable<NotificationDto>> GetUnreadNotificationsAsync(string userId);
        Task MarkAsReadAsync(Guid id);
        Task<IEnumerable<NotificationDto>> GetAllNotificationsAsync(string userId);
    }
}