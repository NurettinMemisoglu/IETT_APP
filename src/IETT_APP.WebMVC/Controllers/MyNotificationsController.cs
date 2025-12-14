using IETT_APP.WebMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IETT_APP.WebMVC.Controllers
{
    [Authorize]
    public class MyNotificationsController : Controller
    {
        private readonly INotificationApiService _notificationService;

        public MyNotificationsController(INotificationApiService notificationService)
        {
            _notificationService = notificationService;
        }

        // GET: /MyNotifications/Index
        // 🔥 EKLEME BURADA: Cache'i kapatıyoruz ki "Geri" tuşunda taze veri gelsin.
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Index()
        {
            try
            {
                var notifications = await _notificationService.GetAllAsync();
                var sortedList = notifications.OrderByDescending(x => x.CreatedAt).ToList();
                return View(sortedList);
            }
            catch
            {
                return View(new List<IETT_APP.Application.Dtos.NotificationDto>());
            }
        }

        // POST: /MyNotifications/MarkRead
        [HttpPost]
        public async Task<IActionResult> MarkRead(Guid id)
        {
            try
            {
                await _notificationService.MarkAsReadAsync(id);
                return Ok();
            }
            catch
            {
                return BadRequest();
            }
        }
    }
}