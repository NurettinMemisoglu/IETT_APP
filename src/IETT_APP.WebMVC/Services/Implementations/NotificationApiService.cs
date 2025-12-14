using IETT_APP.Application.Dtos;
using IETT_APP.WebMVC.Services.Infrastructure; // BaseApiService
using IETT_APP.WebMVC.Services.Interfaces;

namespace IETT_APP.WebMVC.Services.Implementations
{
    public class NotificationApiService : BaseApiService, INotificationApiService
    {
        private readonly HttpClient _httpClient;

        public NotificationApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<NotificationDto>> GetUnreadNotificationsAsync()
        {
            // API: GET api/notifications/unread
            var response = await _httpClient.GetAsync("api/notifications/unread");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<IEnumerable<NotificationDto>>()
                       ?? new List<NotificationDto>();
            }

            return new List<NotificationDto>();
        }

        // 🔥 YENİ EKLENEN METOT 🔥
        public async Task<IEnumerable<NotificationDto>> GetAllAsync()
        {
            // API: GET api/notifications/all
            // (Bu endpointi NotificationsController.cs içinde açmıştık)
            var response = await _httpClient.GetAsync("api/notifications/all");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<IEnumerable<NotificationDto>>()
                       ?? new List<NotificationDto>();
            }

            return new List<NotificationDto>();
        }

        public async Task MarkAsReadAsync(Guid id)
        {
            // API: POST api/notifications/{id}/read
            await _httpClient.PostAsync($"api/notifications/{id}/read", null);
        }
    }
}