using IETT_APP.Application.Dtos.Line;
using IETT_APP.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IETT_APP.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LineController : ControllerBase
    {
        private readonly ILineService<Guid> _lineService;

        public LineController(ILineService<Guid> lineService)
        {
            _lineService = lineService;
        }

        // support filtering: /api/line?active=true
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool? active)
        {
            var lines = await _lineService.GetAllAsync();
            if (active.HasValue)
            {
                lines = lines.Where(l => l.IsActive == active.Value).ToList();
            }
            return Ok(lines);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var line = await _lineService.GetByIdAsync(id);
            if (line == null) return NotFound();
            return Ok(line);
        }

        [HttpPost("execute")]
        public async Task<IActionResult> Execute([FromBody] LineCreateUpdateDto<Guid> dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                if (dto.Id == null || dto.Id == Guid.Empty)
                {
                    var created = await _lineService.CreateAsync(dto);
                    return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
                }

                var updated = await _lineService.UpdateAsync(dto);
                if (!updated) return NotFound();
                return NoContent();
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
            var ok = await _lineService.SetActiveAsync(id, true);
            if (!ok) return NotFound();
            return NoContent();
        }

        // Deactivate a line
        [HttpPatch("{id:guid}/deactivate")]
        public async Task<IActionResult> Deactivate(Guid id)
        {
            var ok = await _lineService.SetActiveAsync(id, false);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _lineService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            var result = await _lineService.SearchAsync(query);
            return Ok(result);
        }
    }
}
