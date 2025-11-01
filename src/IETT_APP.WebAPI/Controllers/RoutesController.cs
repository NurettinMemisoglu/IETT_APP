using IETT_APP.Application.Dtos.Route;
using IETT_APP.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IETT_APP.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoutesController : ControllerBase
    {
        private readonly IRouteService<Guid> _routeService;

        public RoutesController(IRouteService<Guid> routeService)
        {
            _routeService = routeService;
        }

        // support filtering: /api/line?active=true
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool? active)
        {
            var lines = await _routeService.GetAllAsync();
            if (active.HasValue)
            {
                lines = lines.Where(l => l.IsActive == active.Value).ToList();
            }
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
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                if (dto.Id == Guid.Empty)
                {
                    var created = await _routeService.CreateAsync(dto);
                    return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
                }
                var updated = await _routeService.UpdateAsync(dto);
                if (!updated) return NotFound();
                var updatedDto = await _routeService.GetByIdAsync(dto.Id);
                return Ok(updatedDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Activate a line
        [HttpPatch("{id:guid}/activate")]
        public async Task<IActionResult> Activate(Guid id)
        {
            var ok = await _routeService.SetActiveAsync(id, true);
            if (!ok) return NotFound();
            return NoContent();
        }

        // Deactivate a line
        [HttpPatch("{id:guid}/deactivate")]
        public async Task<IActionResult> Deactivate(Guid id)
        {
            var ok = await _routeService.SetActiveAsync(id, false);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
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
