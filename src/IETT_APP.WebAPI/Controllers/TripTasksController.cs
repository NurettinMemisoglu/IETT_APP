using IETT_APP.Application.Dtos.TripTask;
using IETT_APP.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IETT_APP.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripTasksController : ControllerBase
    {
        private readonly ITripTaskService _service;
        // EKLENDİ: DriverService
        private readonly IDriverService _driverService;

        public TripTasksController(ITripTaskService service, IDriverService driverService)
        {
            _service = service;
            _driverService = driverService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? creator = null)
        {
            var list = await _service.GetAllAsync(creator);

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
                return BadRequest(new { message = "API hata", detail = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] TripTaskUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (id != dto.Id) return BadRequest("ID uyuşmazlığı.");

            try
            {
                await _service.UpdateAsync(dto);
                return Ok(await _service.GetByIdAsync(id));
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
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

        // ============================================================
        // ŞOFÖR ÖZEL METOTLARI
        // ============================================================

        // GET: api/triptasks/my-tasks
        [HttpGet("my-tasks")]
        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> GetMyTasks()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // _driverService artık tanımlı, hata vermez.
            var driver = await _driverService.GetByUserIdAsync(userId);
            if (driver == null) return BadRequest("Sürücü profili bulunamadı.");

            var tasks = await _service.GetByDriverIdAsync(driver.Id);
            return Ok(tasks);
        }

        // ============================================================
        // SÜRÜCÜ OPERASYON ENDPOINTLERİ
        // ============================================================

        // 1. KABUL ET
        // PATCH: api/triptasks/{id}/accept
        [HttpPatch("{id}/accept")]
        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> AcceptTrip(Guid id)
        {
            try
            {
                await _service.AcceptTripAsync(id);
                return Ok(new { Message = "Görev kabul edildi." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // 2. REDDET
        // PATCH: api/triptasks/{id}/reject
        [HttpPatch("{id}/reject")]
        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> RejectTrip(Guid id, [FromBody] RejectTripRequestDto dto)
        {
            try
            {
                await _service.RejectTripAsync(id, dto);
                return Ok(new { Message = "Görev reddedildi." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // 3. BAŞLAT
        // PATCH: api/triptasks/{id}/start
        [HttpPatch("{id}/start")]
        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> StartTrip(Guid id)
        {
            try
            {
                await _service.StartTripAsync(id);
                return Ok(new { Message = "Sefer başlatıldı. İyi yolculuklar." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // 4. BİTİR
        // PATCH: api/triptasks/{id}/complete
        [HttpPatch("{id}/complete")]
        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> CompleteTrip(Guid id, [FromBody] CompleteTripRequestDto dto)
        {
            try
            {
                await _service.CompleteTripAsync(id, dto);
                return Ok(new { Message = "Sefer tamamlandı. Geçmiş olsun." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // 5. SORUN BİLDİR
        // PATCH: api/triptasks/{id}/fail
        [HttpPatch("{id}/fail")]
        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> FailTrip(Guid id, [FromBody] FailTripRequestDto dto)
        {
            try
            {
                await _service.FailTripAsync(id, dto);
                return Ok(new { Message = "Durum bildirildi." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // IETT_APP.WebAPI.Controllers.TripTasksController.cs içine ekle

        [HttpGet("dashboard-metrics")]
        public async Task<IActionResult> GetDashboardMetrics([FromQuery] string? username = null)
        {
            try
            {
                // Servisteki yeni metodu çağırıyoruz
                var dashboardData = await _service.GetDashboardMetricsAsync(username);
                return Ok(dashboardData);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Dashboard verisi alınırken hata oluştu.", Detail = ex.Message });
            }
        }
    }
}