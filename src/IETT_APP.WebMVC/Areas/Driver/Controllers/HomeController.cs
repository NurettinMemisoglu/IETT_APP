using IETT_APP.WebMVC.Areas.Driver.Models;
using IETT_APP.WebMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IETT_APP.WebMVC.Areas.Driver.Controllers
{
    [Area("Driver")]
    [Authorize(Roles = "Driver")]
    public class HomeController : Controller
    {
        private readonly IDriverApiService _driverService;

        public HomeController(IDriverApiService driverService)
        {
            _driverService = driverService;
        }

        public async Task<IActionResult> Index()
        {
            // 1. ID'yi almaya çalış
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // 2. ID YOKSA (Ama giriş yapmış görünüyorsa)
            if (string.IsNullOrEmpty(userId))
            {
                // DÖNGÜYÜ KIRAN KOD:
                // Bozuk oturumu temizle (Cookie'yi sil)
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                // Sonra Login'e at
                return RedirectToAction("Login", "Auth", new { area = "" });
            }

            // 3. Her şey yolundaysa veriyi çek
            var driverDto = await _driverService.GetByUserIdAsync(userId);

            var model = new DriverDashboardViewModel
            {
                HasProfile = driverDto != null,
                Profile = driverDto
            };

            return View(model);
        }
    }
}