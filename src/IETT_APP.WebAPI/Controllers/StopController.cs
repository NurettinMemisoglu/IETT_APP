using IETT_APP.Application.Dtos;
using IETT_APP.Application.Interfaces;
using IETT_APP.Applicaton.Dtos.Stop;
using Microsoft.AspNetCore.Mvc;

namespace IETT_APP.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StopsController : ControllerBase
    {
        private readonly IStopService _stopService;

        public StopsController(IStopService stopService)
        {
            _stopService = stopService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StopDto>>> GetAll()
        {
            return Ok(await _stopService.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<StopDto>> GetById(string id)
        {
            var stop = await _stopService.GetByIdAsync(id);
            if (stop == null) return NotFound();
            return Ok(stop);
        }

        [HttpPost]
        public async Task<ActionResult<StopDto>> Create([FromBody] CreateStopDto dto)
        {

            if (!ModelState.IsValid)
            {
                var errors = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return BadRequest($"ModelState hatası: {errors}");
            }

            try
            {
                var stop = await _stopService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = stop.Id }, stop);
            }
            catch (Exception ex)
            {
                return BadRequest($"API Create hata: {ex.Message}");
            }
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateStopDto dto)
        {
            try
            {
                var updated = await _stopService.UpdateAsync(id, dto);
                if (!updated) return NotFound();
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var deleted = await _stopService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<StopDto>>> Search([FromQuery] string name)
        {
            var stops = await _stopService.SearchByNameAsync(name);
            return Ok(stops);
        }
    }
}
