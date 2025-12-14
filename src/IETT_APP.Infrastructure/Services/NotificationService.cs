using AutoMapper;
using IETT_APP.Application.Dtos;
using IETT_APP.Application.Interfaces;
using IETT_APP.Domain.Entities;
using IETT_APP.Domain.Interfaces;
using IETT_APP.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace IETT_APP.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repository;
        private readonly IMapper _mapper;
        // Typed Hub Client kullanıyorsan Interface'i aşağıda güncellemen gerekecek
        private readonly IHubContext<NotificationHub, INotificationClient> _hubContext;

        public NotificationService(INotificationRepository repository, IMapper mapper, IHubContext<NotificationHub, INotificationClient> hubContext)
        {
            _repository = repository;
            _mapper = mapper;
            _hubContext = hubContext;
        }

        public async Task<IEnumerable<NotificationDto>> GetAllNotificationsAsync(string userId)
        {
            var list = await _repository.GetAllByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<NotificationDto>>(list);
        }

        public async Task SendNotificationAsync(string userId, string title, string message, string type = "Info", string? linkUrl = null)
        {
            // 1. Veritabanına Kaydet
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = title,
                Message = message,
                Type = type, // DB'ye "error" veya "warning" olarak kaydediliyor.
                LinkUrl = linkUrl,
                IsRead = false,
                IsActive = true
            };

            await _repository.AddAsync(notification);

            // 2. CANLI BİLDİRİM GÖNDER (SignalR)
            // DÜZELTME: 'type' parametresini ekledik ve sırayı JS ile eşitledik.
            // JS Beklentisi: (title, message, importance, linkUrl, notificationId)
            if (_hubContext != null)
            {
                await _hubContext.Clients.User(userId).ReceiveNotification(
                    title,           // 1. Title
                    message,         // 2. Message
                    type,            // 3. Importance (Type) -> EKSİK OLAN BUYDU
                    linkUrl ?? "",   // 4. LinkUrl
                    notification.Id  // 5. NotificationId
                );
            }
        }

        public async Task<IEnumerable<NotificationDto>> GetUnreadNotificationsAsync(string userId)
        {
            var list = await _repository.GetUnreadByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<NotificationDto>>(list);
        }

        public async Task MarkAsReadAsync(Guid id)
        {
            await _repository.MarkAsReadAsync(id);
        }
    }
}