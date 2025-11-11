using IETT_APP.Application.Interfaces.Garages;
using Microsoft.AspNetCore.Mvc;

namespace IETT_APP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GaragesController : ControllerBase
    {
        private readonly IGarageService _garageService;

        public GaragesController(IGarageService garageService)
        {
            _garageService = garageService;
        }

        // 🔹 GET: api/garages
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _garageService.GetAllAsync();

            if (result == null || !result.Any())
                return NotFound(new { Message = "Hiç garaj bulunamadı." });

            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var line = await _garageService.GetByIdAsync(id);
            if (line == null) return NotFound();
            return Ok(line);
        }
    }
}
