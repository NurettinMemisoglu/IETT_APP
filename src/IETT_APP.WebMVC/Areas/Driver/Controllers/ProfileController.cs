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

        // ============================================================
        // CREATE: Profil Oluşturma (Onboarding)
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

            // Profil yoksa boş form göster
            return View(new CompleteProfileDto());
        }

        // POST: Driver/Profile/Create
        [HttpPost]
        public async Task<IActionResult> Create(CompleteProfileDto model)
        {
            // 1. Validasyon Hatası Varsa JSON Dön
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { message = "Lütfen bilgileri kontrol ediniz.", errors = errors });
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // 2. Servis Çağrısı
            var result = await _driverService.CompleteProfileAsync(userId, model);

            if (result.Succeeded)
            {
                // HATAYI ÇÖZEN KISIM:
                // RedirectToAction YAPMA! JSON dön, yönlendirmeyi JavaScript yapsın.
                return Ok(new
                {
                    message = "Profiliniz başarıyla oluşturuldu.",
                    redirectUrl = Url.Action("Index", "Profile")
                });
            }

            // 3. Servis Hatası Varsa JSON Dön
            var errorMsg = result.Errors != null && result.Errors.Any()
                ? string.Join(", ", result.Errors)
                : "Profil oluşturulurken hata oluştu.";

            return BadRequest(new { message = errorMsg });
        }

        // ============================================================
        // UPDATE: Profil Güncelleme (AJAX)
        // ============================================================
        // POST: Driver/Profile/Update (AJAX)
        [HttpPost]
        public async Task<IActionResult> Update([FromBody] DriverProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Form verileri geçersiz." });

            try
            {
                // Extension metodun ID ataması yapmıyor, bu doğru.
                var updateDto = model.ToUpdateProfileDto();

                // Servise sadece DTO gönderiyoruz. ID göndermiyoruz.
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
        // UPLOAD: Fotoğraf Yükleme (AJAX - Multipart)
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> UploadPhoto(Guid id, IFormFile photo)
        {
            // ... (Validasyonlar) ...

            var result = await _driverService.UploadProfileImageAsync(id, photo);

            if (result.Succeeded)
            {
                // Kullanıcının cookie'sini yenile
                await _sessionService.UpdateProfileImageClaimAsync(result.Data);

                return Ok(new { message = "Fotoğraf güncellendi.", newUrl = result.Data });
            }

            return BadRequest(new { message = "Hata: " + string.Join(", ", result.Errors) });
        }
    }
}