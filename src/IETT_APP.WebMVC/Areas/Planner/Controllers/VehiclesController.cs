using IETT_APP.Domain.Enums;
using IETT_APP.WebMVC.Areas.Planner.Extensions;
using IETT_APP.WebMVC.Areas.Planner.Models;
using IETT_APP.WebMVC.Extensions;
using IETT_APP.WebMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IETT_APP.WebMVC.Areas.Planner.Controllers
{
    [Authorize(Roles = "Planner")]
    [Area("Planner")]
    public class VehiclesController : Controller
    {
        private readonly IVehicleApiService _vehicleApiService;

        public VehiclesController(IVehicleApiService vehicleApiService)
        {
            _vehicleApiService = vehicleApiService;
        }


        // GET: VehiclesController
        public async Task<IActionResult> Index()
        {
            try
            {
                var vehicles = await _vehicleApiService.GetAllAsync();
                var activeVehicles = vehicles
                    .Where(x => !x.IsDeleted)
                    .Select(x => x.ToViewModel())   // <<< DTO → ViewModel
                    .ToList();

                return View(activeVehicles);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Araçlar yüklenemedi: {ex.Message}");
                TempData["ErrorMessage"] = "Araç listesi yüklenirken bir hata oluştu.";
                return View(new List<VehicleViewModel>());  // <<< boş liste gönder
            }
        }

        // GET: VehiclesController/Search
        [HttpGet]
        public async Task<IActionResult> Search(string term)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(term) && term.Length > 100)
                    return BadRequest("Arama terimi çok uzun.");

                var result = string.IsNullOrWhiteSpace(term)
                    ? await _vehicleApiService.GetAllAsync()
                    : await _vehicleApiService.SearchAsync(term.Trim());

                // DTO → ViewModel dönüşümü
                var viewModels = result
                    .Where(x => !x.IsDeleted)
                    .Select(x => x.ToViewModel())
                    .ToList();

                // 🔹 View’a artık yalnızca ViewModel gönderiyoruz
                return PartialView("_VehiclesTablePartial", viewModels);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Araç arama hatası: {ex.Message}");
                return StatusCode(500, "Araç arama sırasında bir hata oluştu.");
            }
        }

        // GET: VehiclesController/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.ServiceStatusList = EnumSelectListHelper.ToSelectList<ServiceStatus>();
            ViewBag.VehicleModelList = EnumSelectListHelper.ToSelectList<VehicleModel>();
            ViewBag.VehicleOperatorList = EnumSelectListHelper.ToSelectList<VehicleOperator>();

            return View(new VehicleViewModel { IsActive = true });
        }

        // POST: VehiclesController/Create
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VehicleViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(kvp => kvp.Value.Errors.Any())
                    .Select(kvp => new { Key = kvp.Key, Errors = kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray() })
                    .ToList();

                return BadRequest(new { message = "Model doğrulama hatası", details = errors });
            }

            var dto = vm.ToCreateDto();

            try
            {
                var result = await _vehicleApiService.CreateAsync(dto);
                return Ok(result); // Güncellenmiş DTO dönülüyor
            }
            catch (Exception ex)
            {
                return BadRequest("API hata: " + ex.Message);
            }
        }

        // GET: VehiclesController/Edit/5
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var vehicle = await _vehicleApiService.GetByIdAsync(id);

                if (vehicle == null || vehicle.IsDeleted)
                    return NotFound();

                // Enum dropdown listeleri
                ViewBag.ServiceStatusList = EnumSelectListHelper.ToSelectList<ServiceStatus>();
                ViewBag.VehicleModelList = EnumSelectListHelper.ToSelectList<VehicleModel>();
                ViewBag.VehicleOperatorList = EnumSelectListHelper.ToSelectList<VehicleOperator>();

                // Modeli ViewModel’e mapleyip view’e gönder
                return View(vehicle.ToViewModel());
            }
            catch (Exception ex)
            {
                // İleri seviye kurumsal versiyonda log eklenir:
                //_logger.LogError(ex, "Araç düzenleme ekranı yüklenemedi, Id: {Id}", id);
                TempData["ErrorMessage"] = "Araç düzenleme sayfası yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Vehicles/Edit
        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] VehicleViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(kvp => kvp.Value.Errors.Any())
                    .Select(kvp => new
                    {
                        Key = kvp.Key,
                        Errors = kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    })
                    .ToList();

                return BadRequest(new { message = "Model doğrulama hatası", details = errors });
            }

            var dto = vm.ToUpdateDto();

            try
            {
                var result = await _vehicleApiService.UpdateAsync(dto.Id, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "API hata", detail = ex.Message });
            }
        }

        // GET: VehiclesController/Delete/{id}
        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var vehicle = await _vehicleApiService.GetByIdAsync(id);
                if (vehicle == null || vehicle.IsDeleted)
                    return NotFound();

                // Onay sayfasına ViewModel gönder
                return View(vehicle.ToViewModel());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Silme sayfası yüklenemedi: {ex.Message}");
                TempData["ErrorMessage"] = "Silme sayfası yüklenirken hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }


        // POST: VehiclesController/Delete/{id}
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                var result = await _vehicleApiService.DeleteAsync(id);
                if (!result)
                {
                    TempData["ErrorMessage"] = "Araç silme işlemi başarısız oldu.";
                    return RedirectToAction(nameof(Index));
                }

                TempData["SuccessMessage"] = "Araç başarıyla silindi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Araç silme hatası: {ex.Message}");
                TempData["ErrorMessage"] = "Araç silinirken beklenmedik bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
