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
                var newId = await _service.AddAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = newId }, dto);
            }
            catch (Exception ex)
            {
                // Hata detayını logla
                Console.WriteLine("TripTask Create Hatası:");
                Console.WriteLine(ex.ToString());

                // Eğer production ise Serilog/NLog kullanabilirsin
                //Log.Error(ex, "TripTask Create Hatası");

                return BadRequest(new { message = "API hata", detail = ex.Message });
            }
        }

        // 🔹 PUT: api/vehicle/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] TripTaskUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!id.Equals(dto.Id))
                return BadRequest(new { Message = "ID mismatch between route and body." });

            // 🔹 Güncellemeyi yap
            await _service.UpdateAsync(dto);

            // 🔹 Güncellenmiş veriyi DB'den tekrar çek
            var updatedDto = await _service.GetByIdAsync(dto.Id);

            return Ok(updatedDto);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, [FromQuery] string? reason = null)
        {
            await _service.DeleteAsync(id, reason);
            return NoContent();
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            var result = await _service.SearchAsync(query);
            return Ok(result);
        }
    }
}
