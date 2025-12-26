using IETT_APP.Application.Dtos.Driver;
using IETT_APP.WebMVC.Areas.Planner.Models;
using IETT_APP.WebMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IETT_APP.WebMVC.Areas.Planner.Controllers
{
    [Area("Planner")]
    [Authorize(Roles = "Admin,Chief,Planner")]
    public class DriversController : Controller
    {
        private readonly IDriverApiService _driverService;
        private readonly IGarageApiService _garageService;

        public DriversController(IDriverApiService driverService, IGarageApiService garageService)
        {
            _driverService = driverService;
            _garageService = garageService;
        }

        // GET: Planner/Drivers
        public async Task<IActionResult> Index()
        {
            try
            {
                var drivers = await _driverService.GetAllAsync();

                if (drivers == null)
                {
                    return View(new List<DriverDto>());
                }

                return View(drivers);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Sürücü listesi hatası: {ex.Message}");
                TempData["ErrorMessage"] = "Veriler yüklenirken bir hata oluştu.";
                return View(new List<DriverDto>());
            }
        }

        // GET: Planner/Drivers/AssignGarage/{id}
        // Bu metot artık MODAL için PartialView döndürüyor
        [HttpGet]
        public async Task<IActionResult> AssignGarage(Guid id)
        {
            try
            {
                // 1. Sürücüyü Getir
                var driver = await _driverService.GetByIdAsync(id);

                if (driver == null)
                {
                    return Content("Sürücü bulunamadı."); // Modal içinde hata mesajı gösterir
                }

                // 2. Garajları Getir
                var garages = await _garageService.GetAllAsync();

                // Güvenli liste oluşturma
                // Not: GarageViewModel kullanıyorsanız Enumerable.Empty<GarageViewModel>() kullanın
                var garageListSafe = garages ?? Enumerable.Empty<GarageViewModel>();

                // 3. Modeli Oluştur
                var model = new AssignGarageViewModel
                {
                    DriverId = driver.Id,
                    DriverFullName = $"{driver.Name ?? ""} {driver.Surname ?? ""}".Trim(),
                    GarageId = driver.GarageId ?? Guid.Empty,
                    // DİKKAT: Partial View ismini ve Property adını doğru kullandık
                    GarageList = new SelectList(garageListSafe, "Id", "GarageName")
                };

                // MODAL İÇİN PARTIAL DÖNÜYORUZ
                return PartialView("_AssignGarageModal", model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AssignGarage GET Hatası: {ex.Message}");
                return Content("Veri yüklenirken hata oluştu: " + ex.Message);
            }
        }

        // POST: Planner/Drivers/AssignGarage
        // Bu metot AJAX ile çalışır
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignGarage(AssignGarageViewModel model)
        {
            // 1. Validasyon Kontrolü
            if (!ModelState.IsValid)
            {
                await ReloadDropdown(model);
                // Hata varsa modalı tekrar (hatalarla birlikte) döndür
                return PartialView("_AssignGarageModal", model);
            }

            try
            {
                // 2. DTO Dönüşümü ve API Çağrısı
                var dto = model.ToDto();
                var result = await _driverService.AssignGarageAsync(dto);

                // 3. BAŞARILI DURUM
                if (result.Succeeded)
                {
                    // AJAX isteği olduğu için JSON dönüyoruz. 
                    // JS tarafı bunu görünce modalı kapatıp sayfayı yenileyecek.
                    return Json(new { success = true, message = result.Message ?? "Garaj ataması başarıyla yapıldı." });
                }

                // 4. BAŞARISIZ DURUM
                if (result.Errors != null && result.Errors.Any())
                {
                    foreach (var error in result.Errors) ModelState.AddModelError("", error);
                }
                else
                {
                    ModelState.AddModelError("", result.Message ?? "Bir hata oluştu.");
                }

                await ReloadDropdown(model);
                // Hataları göstermek için modalı tekrar döndür
                return PartialView("_AssignGarageModal", model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Garaj atama işleminde hata: {ex.Message}");
                ModelState.AddModelError("", "Beklenmedik bir hata oluştu: " + ex.Message);

                await ReloadDropdown(model);
                return PartialView("_AssignGarageModal", model);
            }
        }

        // Dropdown doldurma yardımcısı
        private async Task ReloadDropdown(AssignGarageViewModel model)
        {
            try
            {
                var garages = await _garageService.GetAllAsync();
                var garageListSafe = garages ?? Enumerable.Empty<GarageViewModel>();
                model.GarageList = new SelectList(garageListSafe, "Id", "GarageName");
            }
            catch
            {
                model.GarageList = new SelectList(new List<object>(), "Id", "GarageName");
            }
        }
    }
}