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
        private readonly ITripTaskApiService _tripTaskService;
        private readonly IVehicleApiService _vehicleService;

        // Constructor (Tek ve Temiz)
        public HomeController(
            IDriverApiService driverService,
            ITripTaskApiService tripTaskService,
            IVehicleApiService vehicleService)
        {
            _driverService = driverService;
            _tripTaskService = tripTaskService;
            _vehicleService = vehicleService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                // 1. Dashboard Verisini Çek
                var dashboardDto = await _driverService.GetDashboardAsync();

                // 2. Profil yoksa oluşturmaya gönder
                if (dashboardDto == null)
                {
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
                // API hatasında Create sayfasına yönlendir
                return RedirectToAction("Create", "Profile");
            }
        }

        // ===================================================================
        // Modal İçin Araç Bilgilerini Getirir (JS buradan veri çeker)
        // ===================================================================
        [HttpGet]
        public async Task<IActionResult> GetVehicleInfoForTask(Guid taskId)
        {
            // 1. Task'ı bul
            var task = await _tripTaskService.GetByIdAsync(taskId);

            if (task == null || task.VehicleId == null)
            {
                return Ok(new { LastFuel = (int?)null, LastKm = (int?)null });
            }

            // 2. Aracı bul
            var vehicle = await _vehicleService.GetByIdAsync(task.VehicleId.Value);

            if (vehicle == null)
                return Ok(new { LastFuel = (int?)null, LastKm = (int?)null });

            // 3. JSON dön
            // DİKKAT: VehicleDto içindeki property isimlerinizin FuelLevel ve TotalKm olduğundan emin olun.
            return Ok(new
            {
                LastFuel = vehicle.FuelLevel,
                LastKm = vehicle.TotalKm
            });
        }
    }
}