using IETT_APP.Application.Dtos.TripTask;
using IETT_APP.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IETT_APP.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripTasksController : ControllerBase
    {
        private readonly ITripTaskService _service;

        public TripTasksController(ITripTaskService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _service.GetAllAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TripTaskCreateDto dto)
        {
            try
            {
                var id = await _service.AddAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id }, id);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] TripTaskUpdateDto dto)
        {
            await _service.UpdateAsync(dto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, [FromQuery] string? reason = null)
        {
            await _service.DeleteAsync(id, reason);
            return NoContent();
        }
    }
}
