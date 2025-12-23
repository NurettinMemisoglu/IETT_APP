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
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Index(int page = 1, bool showUnread = false)
        {
            try
            {
                // 1. Tüm Veriyi Çek
                var allNotifications = await _notificationService.GetAllAsync();

                // 2. Sırala (En yeniden en eskiye)
                var sortedList = allNotifications.OrderByDescending(x => x.CreatedAt).ToList();

                // 3. FİLTRELEME (Server-Side)
                if (showUnread)
                {
                    sortedList = sortedList.Where(x => !x.IsRead).ToList();
                }

                // 4. Sayfalama Ayarları
                int pageSize = 20;
                int totalItems = sortedList.Count;
                int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                page = Math.Max(1, Math.Min(page, totalPages > 0 ? totalPages : 1));

                // 5. İlgili Sayfanın Verisini Al
                var pagedData = sortedList.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                // 6. View'a Bilgileri Gönder
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.ShowUnread = showUnread; // Filtre durumu
                ViewBag.UnreadCount = allNotifications.Count(x => !x.IsRead); // Toplam okunmamış (Rozet için)

                return View(pagedData);
            }
            catch
            {
                return View(new List<IETT_APP.Application.Dtos.NotificationDto>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkRead(Guid id)
        {
            try { await _notificationService.MarkAsReadAsync(id); return Ok(); } catch { return BadRequest(); }
        }
    }
}