using IETT_APP.WebMVC.Areas.Admin.Models;
using IETT_APP.WebMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IETT_APP.WebMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly IApiUserService _userService;
        // İleride IGarageService, IVehicleService vb. eklenecek

        public HomeController(IApiUserService userService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAllAsync();

            var model = new DashboardViewModel
            {
                TotalUserCount = users.Count(),

                // SİHİRLİ KISIM BURASI:
                RoleCounts = users
                    .Where(u => u.RoleNames != null)       // Rolü olmayanları ele (Null check)
                    .SelectMany(u => u.RoleNames)          // Tüm kullanıcıların rollerini tek bir listeye dök
                    .GroupBy(r => r)                       // Rol ismine göre grupla
                    .ToDictionary(g => g.Key, g => g.Count()) // Sözlüğe çevir: Key=RolAdı, Value=Sayı
            };

            return View(model);
        }
    }
}