using IETT_APP.Domain.Enums;
using IETT_APP.WebMVC.Areas.Chief.Extensions;
using IETT_APP.WebMVC.Areas.Chief.Models;
using IETT_APP.WebMVC.Extensions;
using IETT_APP.WebMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;

namespace IETT_APP.WebMVC.Areas.Chief.Controllers
{
    [Authorize(Roles = "Chief, Admin")]
    [Area("Chief")]
    public class TripTasksController : Controller
    {
        private readonly ITripTaskApiService _tripTaskApiService;
        private readonly ILineApiService _lineService;
        private readonly IRouteApiService _routeService;
        private readonly IVehicleApiService _vehicleService;
        private readonly IGarageApiService _garageService;
        private readonly IDriverApiService _driverService;

        public TripTasksController(
            ITripTaskApiService tripTaskApiService,
            ILineApiService lineService,
            IRouteApiService routeService,
            IVehicleApiService vehicleService,
            IGarageApiService garageService,
            IDriverApiService driverService)
        {
            _tripTaskApiService = tripTaskApiService;
            _lineService = lineService;
            _routeService = routeService;
            _vehicleService = vehicleService;
            _garageService = garageService;
            _driverService = driverService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var tasks = await _tripTaskApiService.GetAllAsync();
                var activeTasks = tasks.Where(x => !x.IsDeleted).Select(x => x.ToViewModel()).ToList();
                return View(activeTasks);
            }
            catch
            {
                TempData["ErrorMessage"] = "Görev listesi yüklenirken hata oluştu.";
                return View(new List<TripTaskViewModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Search(string term)
        {
            try
            {
                var result = string.IsNullOrWhiteSpace(term)
                    ? await _tripTaskApiService.GetAllAsync()
                    : await _tripTaskApiService.SearchAsync(term.Trim());

                var viewModels = result.Where(x => !x.IsDeleted).Select(x => x.ToViewModel()).ToList();
                return PartialView("_TripTasksTablePartial", viewModels);
            }
            catch
            {
                return StatusCode(500, "Arama hatası.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var task = await _tripTaskApiService.GetByIdAsync(id);
                if (task == null || task.IsDeleted) return NotFound();
                return View(task.ToViewModel());
            }
            catch
            {
                TempData["ErrorMessage"] = "Detaylar yüklenirken hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Yeni kayıtta statü yoktur (null)
            await LoadDropdownData(null);
            return View(new TripTaskViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TripTaskViewModel vm)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Form verileri hatalı." });

            var dto = vm.ToCreateDto();

            try
            {
                var result = await _tripTaskApiService.CreateAsync(dto);
                if (result.Succeeded)
                {
                    return Ok(new { message = "Görev başarıyla oluşturuldu.", redirectUrl = Url.Action("Index") });
                }
                return BadRequest(new { message = result.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Sistem hatası: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var task = await _tripTaskApiService.GetByIdAsync(id);
                if (task == null || task.IsDeleted) return NotFound();

                // Mevcut durumu gönderiyoruz
                await LoadDropdownData(task.Status);

                return View(task.ToViewModel());
            }
            catch
            {
                TempData["ErrorMessage"] = "Düzenleme sayfası açılamadı.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] TripTaskViewModel vm)
        {
            if (!ModelState.IsValid) return BadRequest(new { message = "Form verileri hatalı." });

            if ((vm.Status == TaskState.Cancelled || vm.Status == TaskState.Incomplete) &&
                string.IsNullOrWhiteSpace(vm.StatusReason))
            {
                return BadRequest(new { message = "Görevi iptal ederken veya yarım bırakırken 'Durum Açıklaması' girmek zorunludur." });
            }

            var dto = vm.ToUpdateDto();

            try
            {
                var result = await _tripTaskApiService.UpdateAsync(dto.Id, dto);
                if (result.Succeeded)
                {
                    return Ok(new { message = "Görev güncellendi.", redirectUrl = Url.Action("Index") });
                }
                return BadRequest(new { message = result.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Hata: " + ex.Message });
            }
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                var result = await _tripTaskApiService.DeleteAsync(id);
                if (!result.Succeeded)
                {
                    return BadRequest(new { message = result.Message });
                }
                return Ok(new { message = "Görev silindi." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Hata: " + ex.Message });
            }
        }

        // ==============================
        // YARDIMCI METOT (DÜZELTİLDİ)
        // ==============================

        // Parametre eklendi: TaskState? currentStatus = null
        private async Task LoadDropdownData(TaskState? currentStatus = null)
        {
            var linesTask = _lineService.GetAllAsync();
            var routesTask = _routeService.GetAllAsync();
            var garagesTask = _garageService.GetAllAsync();
            var vehiclesTask = _vehicleService.GetAllAsync();
            var driversTask = _driverService.GetAllAsync();

            await Task.WhenAll(linesTask, routesTask, garagesTask, vehiclesTask, driversTask);

            var lines = linesTask.Result;
            var routes = routesTask.Result;
            var garages = garagesTask.Result;
            var vehicles = vehiclesTask.Result.Where(v => v.IsActive).ToList();
            var drivers = driversTask.Result.Where(d => d.IsActive).ToList();

            // İzin verilen durumları çek
            var allowedStates = _tripTaskApiService.GetAllowedStatesForRole("Chief");

            // Eğer mevcut durum listede yoksa ekle (Edit modunda veri kaybolmasın diye)
            if (currentStatus.HasValue && !allowedStates.Contains(currentStatus.Value))
            {
                allowedStates.Add(currentStatus.Value);
            }

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            ViewBag.LinesJson = JsonSerializer.Serialize(lines, jsonOptions);
            ViewBag.RoutesJson = JsonSerializer.Serialize(routes, jsonOptions);
            ViewBag.GaragesJson = JsonSerializer.Serialize(garages, jsonOptions);
            ViewBag.VehiclesJson = JsonSerializer.Serialize(vehicles, jsonOptions);
            ViewBag.DriversJson = JsonSerializer.Serialize(drivers, jsonOptions);

            ViewBag.LineList = new SelectList(lines, "Id", "Code");
            ViewBag.GarageList = new SelectList(garages, "Id", "GarageName");

            // TaskStateList'i filtrelenmiş listeden oluşturuyoruz
            ViewBag.TaskStateList = new SelectList(allowedStates.Select(s => new
            {
                Value = (int)s,
                Text = s.ToDisplayName()
            }), "Value", "Text");
        }
    }
}