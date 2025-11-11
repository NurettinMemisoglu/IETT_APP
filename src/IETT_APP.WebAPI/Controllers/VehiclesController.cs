using IETT_APP.Application.Dtos.Vehicle;
using IETT_APP.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IETT_APP.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehiclesController : ControllerBase
    {
        private readonly IVehicleService<Guid> _vehicleService;

        public VehiclesController(IVehicleService<Guid> vehicleService)
        {
            _vehicleService = vehicleService;
        }

        // 🔹 GET: api/vehicle
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _vehicleService.GetAllAsync();
            return Ok(result);
        }

        // 🔹 GET: api/vehicle/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _vehicleService.GetByIdAsync(id);
            if (result == null)
                return NotFound(new { Message = $"Vehicle with ID {id} not found." });

            return Ok(result);
        }

        // 🔹 POST: api/vehicle
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VehicleCreateDto<Guid> dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var newId = await _vehicleService.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = newId }, dto);
        }

        // 🔹 PUT: api/vehicle/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] VehicleUpdateDto<Guid> dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!id.Equals(dto.Id))
                return BadRequest(new { Message = "ID mismatch between route and body." });

            // 🔹 Güncellemeyi yap
            await _vehicleService.UpdateAsync(dto);

            // 🔹 Güncellenmiş veriyi DB'den tekrar çek
            var updatedDto = await _vehicleService.GetByIdAsync(dto.Id);

            return Ok(updatedDto);
        }

        // 🔹 DELETE: api/vehicle/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _vehicleService.DeleteAsync(id);
            if (!deleted)
                return NotFound(new { Message = $"Vehicle with ID {id} not found or already deleted." });

            return NoContent();
        }

        // 🔹 GET: api/vehicle/search?query=34ABC123
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            var result = await _vehicleService.SearchAsync(query);
            return Ok(result);
        }

        // GET: /api/vehicles/unassigned
        [HttpGet("unassigned")]
        public async Task<IActionResult> GetUnassigned()
        {
            var vehicles = await _vehicleService.GetUnassignedVehiclesAsync();
            return Ok(vehicles);
        }


        // POST: /api/vehicles/unassign/{id}
        [HttpPost("unassign/{id:guid}")]
        public async Task<IActionResult> UnassignFromLine(Guid id)
        {
            try
            {
                await _vehicleService.UnassignFromLineAsync(id);
                return Ok(new { Message = "Araç hattan kaldırıldı." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ex.Message });
            }
        }
    }

}
