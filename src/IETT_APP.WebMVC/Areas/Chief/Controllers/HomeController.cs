using IETT_APP.WebMVC.Areas.Chief.Extensions; // Extension'ı buraya ekledik
using IETT_APP.WebMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IETT_APP.WebMVC.Areas.Chief.Controllers
{
    [Authorize(Roles = "Chief, Admin")]
    [Area("Chief")]
    public class HomeController : Controller
    {
        private readonly ITripTaskApiService _tripTaskService;

        public HomeController(ITripTaskApiService tripTaskService)
        {
            _tripTaskService = tripTaskService;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Kullanıcıyı Belirle
            var username = User.IsInRole("Admin") ? null : User.Identity?.Name;

            // 2. API'den Hazır Paketi Al (DTO)
            var dashboardDto = await _tripTaskService.GetChiefDashboardMetricsAsync(username);

            // 3. DTO'yu ViewModel'e Çevir (Extension Metoduyla)
            var model = dashboardDto.ToViewModel();

            // 4. View'e Gönder
            return View(model);
        }
    }
}