using IETT_APP.WebMVC.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IETT_APP.WebMVC.ViewComponents
{
    public class NotificationBellViewComponent : ViewComponent
    {
        private readonly INotificationApiService _notificationService;

        public NotificationBellViewComponent(INotificationApiService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var notifications = await _notificationService.GetUnreadNotificationsAsync();
            return View(notifications);
        }
    }
}