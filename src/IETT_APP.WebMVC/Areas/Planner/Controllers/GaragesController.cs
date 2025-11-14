using IETT_APP.WebMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IETT_APP.WebMVC.Areas.Planner.Controllers
{
    [Authorize(Roles = "Planner")]
    [Area("Planner")]
    public class GaragesController : Controller
    {
        private readonly IGarageApiService _garageApiService;

        public GaragesController(IGarageApiService garageApiService)
        {
            _garageApiService = garageApiService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var garages = await _garageApiService.GetAllAsync();
            return Json(garages);
        }
    }
}
