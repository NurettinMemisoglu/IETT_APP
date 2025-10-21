using IETT_APP.Application.Dtos.Route;
using IETT_APP.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IETT_APP.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RouteController : ControllerBase
    {
        private readonly IRouteService<Guid> _routeService;

        public RouteController(IRouteService<Guid> routeService)
        {
            _routeService = routeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var lines = await _routeService.GetAllAsync();
            return Ok(lines);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var line = await _routeService.GetByIdAsync(id);
            if (line == null) return NotFound();
            return Ok(line);
        }

        [HttpPost("execute")]
        public async Task<IActionResult> Execute([FromBody] RouteCreateUpdateDto<Guid> dto)
        {
            try
            {
                if (dto.Id == null || dto.Id == Guid.Empty)
                {
                    var created = await _routeService.CreateAsync(dto);
                    return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
                }

                var updated = await _routeService.UpdateAsync(dto);
                if (!updated) return NotFound();
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _routeService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            var result = await _routeService.SearchAsync(query);
            return Ok(result);
        }
    }
}
