using IETT_APP.Domain.Enums;
using IETT_APP.WebMVC.Areas.Planner.Models;
using IETT_APP.WebMVC.Extensions;
using IETT_APP.WebMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IETT_APP.WebMVC.Areas.Planner.Controllers
{
    [Authorize(Roles = "Planner")]
    [Area("Planner")]
    public class RoutesController : Controller
    {
        private readonly IRouteApiService _routeApiService;

        public RoutesController(IRouteApiService routeApiService)
        {
            _routeApiService = routeApiService;
        }

        // Ana sayfa
        public async Task<IActionResult> Index()
        {
            var routes = await _routeApiService.GetAllAsync();

            Console.WriteLine("=== ROUTE LIST DEBUG ===");
            foreach (var route in routes)
            {
                Console.WriteLine($"Route: {route.Name} ({route.Code})");

                if (route.Stops != null && route.Stops.Any())
                {
                    foreach (var stop in route.Stops)
                    {
                        Console.WriteLine($"Stop: {stop.Name} (Lat: {stop.Latitude}, Lng: {stop.Longitude})");
                    }
                }
                else
                {
                    Console.WriteLine("Stops: Yok");
                }

                Console.WriteLine("--------------");
            }

            return View(routes.Where(x => !x.IsDeleted).Select(x => x.ToViewModel()).ToList());
        }
        // Arama
        [HttpGet]
        public async Task<IActionResult> Search(string term)
        {
            var result = string.IsNullOrWhiteSpace(term)
                ? await _routeApiService.GetAllAsync()
                : await _routeApiService.SearchAsync(term);

            result = result.Where(x => !x.IsDeleted).ToList();

            var viewModels = result.Select(x => x.ToViewModel()).ToList();
            return PartialView("_RoutesTablePartial", viewModels);
        }

        // Yeni route formu
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.RoutesTypeList = GetRoutesTypeSelectList();

            return View(new RouteViewModel { IsActive = true });
        }

        // Düzenleme formu
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var route = await _routeApiService.GetByIdAsync(id);
            if (route == null || route.IsDeleted)
                return NotFound();

            ViewBag.RoutesTypeList = GetRoutesTypeSelectList();
            return View("Edit", route.ToViewModel());
        }

        // Create / Update işlemi
        [HttpPost]
        public async Task<IActionResult> Execute([FromBody] RouteViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(kvp => kvp.Value.Errors.Any())
                    .Select(kvp => new { Key = kvp.Key, Errors = kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray() })
                    .ToList();

                return BadRequest(new { message = "Model doğrulama hatası", details = errors });
            }

            var dto = vm.ToDto();

            try
            {
                var result = await _routeApiService.CreateOrUpdateAsync(dto);
                return Ok(result); // Güncellenmiş DTO dönülüyor
            }
            catch (Exception ex)
            {
                return BadRequest("API hata: " + ex.Message);
            }
        }

        // Silme
        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _routeApiService.DeleteAsync(id);
            if (!result) return BadRequest("Silme işlemi başarısız.");
            return Ok(new { message = "Route başarıyla silindi." });
        }

        // RouteType için select list
        private SelectList GetRoutesTypeSelectList() => new SelectList(
            Enum.GetValues(typeof(RoutesDirection)).Cast<RoutesDirection>()
                .Select(x => new { Value = (int)x, Text = x.ToDisplayName() }),
            "Value", "Text"
        );
    }
}
