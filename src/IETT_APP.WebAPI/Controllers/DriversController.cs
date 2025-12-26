using IETT_APP.Application.Dtos.Driver;
using IETT_APP.Application.Interfaces;
using IETT_APP.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace IETT_APP.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DriversController : ControllerBase
    {
        private readonly IDriverService _driverService;

        public DriversController(IDriverService driverService)
        {
            _driverService = driverService;
        }

        // ============================================================
        // OKUMA İŞLEMLERİ
        // ============================================================

        [HttpGet]
        [Authorize(Roles = "Admin,Chief,Planner")]
        public async Task<ActionResult<IEnumerable<DriverDto>>> GetAll()
        {
            return Ok(await _driverService.GetAllAsync());
        }

        [HttpGet("unassigned")]
        [Authorize(Roles = "Admin,Chief,Planner")]
        public async Task<ActionResult<IEnumerable<DriverDto>>> GetUnassigned()
        {
            return Ok(await _driverService.GetUnassignedDriversAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DriverDto>> GetById(Guid id)
        {
            var driver = await _driverService.GetByIdAsync(id);
            if (driver == null) return NotFound("Sürücü bulunamadı.");
            return Ok(driver);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<DriverDto>> GetByUserId(string userId)
        {
            var driver = await _driverService.GetByUserIdAsync(userId);
            if (driver == null) return NotFound("Bu kullanıcıya ait sürücü kaydı yok.");
            return Ok(driver);
        }

        // ============================================================
        // YÖNETİM İŞLEMLERİ (Admin/Chief)
        // ============================================================

        // Tam Yetkili Güncelleme (Admin)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDriverDto dto)
        {
            if (id != dto.Id) return BadRequest("ID uyuşmazlığı.");

            try
            {
                await _driverService.UpdateAsync(dto);
                return Ok(new { Message = "Sürücü tam yetkiyle güncellendi." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<DriverDto>> Create([FromBody] CreateDriverDto dto)
        {
            try
            {
                var result = await _driverService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _driverService.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPatch("assign-garage")]
        [Authorize(Roles = "Admin,Chief,Planner")]
        public async Task<IActionResult> AssignGarage([FromBody] AssignGarageDto dto)
        {
            try
            {
                await _driverService.AssignGarageAsync(dto);
                return Ok(new { Message = "Garaj ataması başarılı." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // ============================================================
        // ŞOFÖR İŞLEMLERİ (Kısıtlı Yetki)
        // ============================================================

        // --- YENİ EKLENEN: Kısıtlı Profil Güncelleme ---
        // Şoför sadece kendine ait Telefon, Adres vb. güncelleyebilir.
        [HttpPatch("profile")]
        [Authorize(Roles = "Driver")] // Sadece şoförler
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateDriverProfileDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // 1. Token'dan User ID'yi al (En Güvenli Yöntem)
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            // 2. Bu User ID'ye ait Driver profilini bul
            var currentDriver = await _driverService.GetByUserIdAsync(currentUserId);
            if (currentDriver == null) return NotFound("Sürücü profili bulunamadı.");

            try
            {
                // 3. Servise "Bu ID'li şoförü, şu verilerle güncelle" de.
                var result = await _driverService.UpdateProfileAsync(currentDriver.Id, dto);

                return Ok(new { Message = "Profil bilgileriniz güncellendi.", Data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // Şoförün Profilini İlk Kez Oluşturması (Onboarding)
        // POST: api/drivers/complete-profile
        [HttpPost("complete-profile")]
        [Authorize(Roles = "Driver")]
        [Consumes("multipart/form-data")] // Dosya yükleme olduğu için şart
        public async Task<ActionResult<DriverDto>> CompleteProfile([FromForm] CompleteProfileRequest request)
        {
            // 1. Kullanıcı Kimliği Kontrolü
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // 2. JSON String'i DTO'ya Çevirme (Deserialization)
            CompleteProfileDto? dto;
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                dto = JsonSerializer.Deserialize<CompleteProfileDto>(request.Data, options);
            }
            catch (JsonException)
            {
                return BadRequest(new { Message = "Gönderilen veri formatı (JSON) hatalı." });
            }

            if (dto == null) return BadRequest(new { Message = "Veri boş olamaz." });

            //DOSYALARI DTO'YA ATAMA
            // Bunu yapmazsan TryValidateModel hep hata verir çünkü dosyalar [Required].
            dto.LicenseDocument = request.LicenseDocument;
            dto.PsychotechnicDocument = request.PsychotechnicDocument;

            // 3. Manuel Validasyon (Çünkü DTO otomatik bind edilmedi)
            if (!TryValidateModel(dto))
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new { Field = x.Key, Error = x.Value.Errors.First().ErrorMessage })
                    .ToList();

                // Hatanın ne olduğunu JSON olarak dönüyoruz
                return BadRequest(new { Message = "Validasyon Hatası", Errors = errors });
            }

            try
            {
                // 4. Servise Gönderim (Veri + Dosyalar Ayrı Ayrı)
                var result = await _driverService.CompleteProfileAsync(
                    userId,
                    dto,
                    request.LicenseDocument,
                    request.PsychotechnicDocument
                );

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // Profil Fotoğrafı Yükleme
        [HttpPost("{id}/upload-photo")]
        [Authorize]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadPhoto(
            [FromRoute] Guid id,
            [FromForm] IFormFile photo)
        {
            if (photo == null || photo.Length == 0)
                return BadRequest(new { Message = "Lütfen bir resim dosyası seçiniz." });

            if (id == Guid.Empty)
                return BadRequest(new { Message = "Geçersiz ID." });

            var dto = new UploadProfileImageDto { Photo = photo };

            try
            {
                // Güvenlik: Başkası değiştiremesin (Admin hariç)
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

                if (!userRoles.Contains("Admin"))
                {
                    var currentDriver = await _driverService.GetByUserIdAsync(currentUserId!);
                    if (currentDriver == null || currentDriver.Id != id)
                    {
                        return Forbid();
                    }
                }

                var imagePath = await _driverService.UploadProfileImageAsync(id, dto);
                return Ok(new { Message = "Profil fotoğrafı güncellendi.", Path = imagePath });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // ============================================================
        // 📱 MOBİL / DASHBOARD API
        // ============================================================

        // GET: api/drivers/dashboard
        [HttpGet("dashboard")]
        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> GetDashboard()
        {
            try
            {
                // Token'dan User ID'yi güvenli şekilde al
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("Kullanıcı kimliği doğrulanamadı.");

                var dashboardData = await _driverService.GetDriverDashboardAsync(userId);

                return Ok(dashboardData);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Dashboard yüklenirken hata oluştu.", Detail = ex.Message });
            }
        }
    }
}