using IETT_APP.WebMVC.Areas.Driver.Models;
using IETT_APP.WebMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                // 1. API'den Dashboard Verisini Çekmeye Çalış
                var dashboardDto = await _driverService.GetDashboardAsync();

                // 2. Eğer dashboardDto NULL geliyorsa, API'den veri alamadık demektir.
                // Bu genellikle yeni kullanıcının profili olmadığı anlamına gelir.
                if (dashboardDto == null)
                {
                    // HATAYI ÇÖZEN SATIR BURASI:
                    // Login'e atmak yerine, Profil Oluşturma sayfasına yönlendiriyoruz.
                    return RedirectToAction("Create", "Profile");
                }

                // 3. Veri geldiyse Dashboard'ı göster
                var model = new DriverDashboardViewModel
                {
                    HasProfile = true,
                    DashboardData = dashboardDto
                };

                return View(model);
            }
            catch (Exception)
            {
                // API 400 veya 404 hatası fırlattıysa (Profil yoksa API hata fırlatıyor olabilir)
                // Bu durumda da Create sayfasına yönlendirmeliyiz.

                // NOT: Eğer API gerçekten çöktüyse kullanıcı Create sayfasına gider, 
                // orası da hata verirse ExceptionMiddleware yakalar.
                // Ancak "Döngüye girmemesi" için en güvenli yol budur.
                return RedirectToAction("Create", "Profile");
            }
        }
    }
}