using IETT_APP.Application.Dtos.TripTask;
using IETT_APP.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IETT_APP.WebAPI.Controllers
{
    [Route("api/driver-tasks")]
    [ApiController]
    [Authorize(Roles = "Driver")]
    public class DriverTasksController : ControllerBase
    {
        private readonly ITripTaskService _tripTaskService;

        // EKLENEN KISIM 1: DriverService Tanımı
        private readonly IDriverService _driverService;

        // EKLENEN KISIM 2: Constructor'a IDriverService Eklendi
        public DriverTasksController(ITripTaskService tripTaskService, IDriverService driverService)
        {
            _tripTaskService = tripTaskService;
            _driverService = driverService; // Atama yapıldı
        }

        // =================================================================
        // 1. GÖREV KABUL ET
        // POST: api/driver-tasks/accept/{id}
        // =================================================================
        [HttpPost("accept/{id}")]
        public async Task<IActionResult> Accept(Guid id)
        {
            try
            {
                await _tripTaskService.AcceptTripAsync(id);
                return Ok(new { Message = "Görev kabul edildi, iyi yolculuklar!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // =================================================================
        // 2. GÖREV REDDET
        // POST: api/driver-tasks/reject/{id}
        // =================================================================
        [HttpPost("reject/{id}")]
        public async Task<IActionResult> Reject(Guid id, [FromBody] RejectTripRequestDto request)
        {
            try
            {
                await _tripTaskService.RejectTripAsync(id, request);
                return Ok(new { Message = "Görev reddedildi." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // =================================================================
        // 3. SEFERİ BAŞLAT
        // POST: api/driver-tasks/start/{id}
        // =================================================================
        [HttpPost("start/{id}")]
        public async Task<IActionResult> Start(Guid id)
        {
            try
            {
                await _tripTaskService.StartTripAsync(id);
                return Ok(new { Message = "Sefer başlatıldı. Güvenli sürüşler." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // =================================================================
        // 4. SEFERİ TAMAMLA
        // POST: api/driver-tasks/complete/{id}
        // =================================================================
        [HttpPost("complete/{id}")]
        public async Task<IActionResult> Complete(Guid id, [FromBody] CompleteTripRequestDto request)
        {
            try
            {
                await _tripTaskService.CompleteTripAsync(id, request);
                return Ok(new { Message = "Sefer başarıyla tamamlandı." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // =================================================================
        // 5. SORUN BİLDİR (ARIZA/KAZA)
        // POST: api/driver-tasks/fail/{id}
        // =================================================================
        [HttpPost("fail/{id}")]
        public async Task<IActionResult> Fail(Guid id, [FromBody] FailTripRequestDto request)
        {
            try
            {
                await _tripTaskService.FailTripAsync(id, request);
                return Ok(new { Message = "Sorun bildirildi ve sefer sonlandırıldı." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // =================================================================
        // 6. ŞOFÖRÜN GÖREVLERİNİ GETİR (GET)
        // GET: api/driver-tasks/my-tasks
        // =================================================================
        [HttpGet("my-tasks")]
        public async Task<ActionResult<IEnumerable<TripTaskDto>>> GetMyTasks()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                // Artık _driverService tanımlı olduğu için burası çalışacak
                var driver = await _driverService.GetByUserIdAsync(userId);

                if (driver == null) return NotFound("Sürücü profili bulunamadı.");

                var tasks = await _tripTaskService.GetByDriverIdAsync(driver.Id);

                return Ok(tasks);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}