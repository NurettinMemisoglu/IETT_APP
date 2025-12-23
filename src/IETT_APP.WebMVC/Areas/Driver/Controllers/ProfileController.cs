using IETT_APP.Application.Dtos.Driver;
using IETT_APP.WebMVC.Areas.Driver.Extensions;
using IETT_APP.WebMVC.Areas.Driver.Models;
using IETT_APP.WebMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IETT_APP.WebMVC.Areas.Driver.Controllers
{
    [Area("Driver")]
    [Authorize(Roles = "Driver")]
    public class ProfileController : Controller
    {
        private readonly IDriverApiService _driverService;
        private readonly IUserSessionApiService _sessionService;

        public ProfileController(IDriverApiService driverService, IUserSessionApiService sessionService)
        {
            _driverService = driverService;
            _sessionService = sessionService;
        }

        // ============================================================
        // INDEX: Profil Görüntüleme
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Auth", new { area = "" });

                var driver = await _driverService.GetByUserIdAsync(userId);

                // Profil YOKSA -> Oluşturmaya Gönder
                if (driver == null)
                {
                    return RedirectToAction("Create");
                }

                var model = driver.ToViewModel();
                return View(model);
            }
            catch (Exception ex)
            {
                // Index'te hata olursa bembeyaz sayfa yerine Error sayfasına veya güvenli bir yere atabiliriz
                TempData["ErrorMessage"] = "Profil yüklenirken bir sorun oluştu: " + ex.Message;
                return RedirectToAction("Index", "Home", new { area = "" });
            }
        }

        // ============================================================
        // CREATE: Profil Oluşturma (Form Görüntüleme)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Auth", new { area = "" });

            // Kontrol: Zaten profili varsa tekrar oluşturmasın
            var existingDriver = await _driverService.GetByUserIdAsync(userId);
            if (existingDriver != null)
            {
                TempData["InfoMessage"] = "Profiliniz zaten mevcut.";
                return RedirectToAction("Index");
            }

            return View(new CompleteProfileDto());
        }

        // ============================================================
        // CREATE POST: (DÜZELTİLDİ - ARTIK SAYFAYI PATLATMIYOR)
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> Create(CompleteProfileDto model)
        {
            // 1. Model Validasyon
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { message = "Lütfen bilgileri kontrol ediniz.", errors = errors });
            }

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                // 2. Servis Çağrısı
                var result = await _driverService.CompleteProfileAsync(userId, model);

                // 3. SONUÇ KONTROLÜ
                if (result.Succeeded)
                {
                    return Ok(new
                    {
                        message = "Profiliniz başarıyla oluşturuldu. Yönlendiriliyorsunuz...",
                        redirectUrl = Url.Action("Index", "Profile")
                    });
                }

                // --- TEMİZ KOD ---
                // Artık hata analiz etmiyoruz, servisin hazırladığı temiz mesajı basıyoruz.
                return BadRequest(new { message = result.Message });
            }
            catch (Exception ex)
            {
                // Sadece bağlantı kopması vb. beklenmedik durumlarda burası çalışır
                return BadRequest(new { message = "Beklenmedik bir hata oluştu: " + ex.Message });
            }
        }

        // ============================================================
        // UPDATE: Profil Güncelleme (AJAX - JSON Döner)
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> Update([FromBody] DriverProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Form verileri geçersiz." });

            try
            {
                var updateDto = model.ToUpdateProfileDto();
                var result = await _driverService.UpdateProfileAsync(updateDto);

                if (result.Succeeded)
                    return Ok(new { message = "Profil bilgileri güncellendi." });
                else
                    return BadRequest(new { message = "Hata: " + string.Join(", ", result.Errors) });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Güncelleme sırasında hata oluştu: " + ex.Message });
            }
        }

        // ============================================================
        // UPLOAD: Fotoğraf Yükleme (AJAX - JSON Döner)
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> UploadPhoto(Guid id, IFormFile photo)
        {
            try
            {
                var result = await _driverService.UploadProfileImageAsync(id, photo);

                if (result.Succeeded)
                {
                    await _sessionService.UpdateProfileImageClaimAsync(result.Data);
                    return Ok(new { message = "Fotoğraf güncellendi.", newUrl = result.Data });
                }

                return BadRequest(new { message = "Hata: " + string.Join(", ", result.Errors) });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Fotoğraf yüklenirken hata oluştu: " + ex.Message });
            }
        }
    }
}