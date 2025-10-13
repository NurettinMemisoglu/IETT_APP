using IETT_APP.WebMVC.Areas.Planner.Models;
using IETT_APP.WebMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IETT_APP.WebMVC.Areas.Planner.Controllers
{
    [Authorize(Roles = "Planner")]
    [Area("Planner")]
    public class HomeController : Controller
    {
        private readonly IStopService _stopService;

        public HomeController(IStopService stopService)
        {
            _stopService = stopService;
        }

        // GET: Planner/Home/Index
        public async Task<IActionResult> Index(string? search)
        {
            // Arama varsa filtreli getir, yoksa tüm durakları çek
            var stopDtos = string.IsNullOrWhiteSpace(search)
                ? await _stopService.GetAllAsync()
                : await _stopService.SearchByNameAsync(search);

            // DTO → ViewModel dönüşümü
            var stops = stopDtos.Select(dto => new StopViewModel
            {
                Id = dto.Id,
                Code = dto.Code,
                Name = dto.Name,
                Type = dto.Type,
                Location = new LocationViewModel
                {
                    Latitude = dto.Location?.Latitude ?? 0,
                    Longitude = dto.Location?.Latitude ?? 0
                }
            }).ToList();

            ViewData["Search"] = search; // formda arama kutusuna geri yazabilmek için
            return View(stops);
        }
    }
}
