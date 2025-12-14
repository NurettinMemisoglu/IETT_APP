using IETT_APP.Domain.Entities;

namespace IETT_APP.Domain.Interfaces
{
    public interface INotificationRepository
    {
        Task AddAsync(Notification notification);
        Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(string userId);
        Task MarkAsReadAsync(Guid notificationId);
        Task<IEnumerable<Notification>> GetAllByUserIdAsync(string userId);
    }
}