using IETT_APP.Application.Dtos.TripTask;
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
    public class TasksController : Controller
    {
        private readonly ITripTaskApiService _tripTaskApiService;
        private readonly IDriverApiService _driverService;

        public TasksController(ITripTaskApiService tripTaskApiService, IDriverApiService driverService)
        {
            _tripTaskApiService = tripTaskApiService;
            _driverService = driverService;
        }

        // ============================================================
        // 1. GÖREV LİSTESİ (SAYFA)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // --- GÜVENLİK KONTROLÜ BAŞLANGICI ---
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Profil var mı kontrol et
            var driver = await _driverService.GetByUserIdAsync(userId);
            if (driver == null)
            {
                TempData["ErrorMessage"] = "Görevlerinizi görmek için önce profilinizi tamamlamalısınız.";
                // Profil yoksa direkt Profil Oluşturma sayfasına postala
                return RedirectToAction("Create", "Profile");
            }
            // --- GÜVENLİK KONTROLÜ BİTİŞİ ---

            try
            {
                var taskDtos = await _tripTaskApiService.GetMyTasksAsync();

                var viewModels = taskDtos
                    .OrderBy(t => t.ScheduledDeparture)
                    .Select(t => t.ToDriverViewModel())
                    .ToList();

                return View(viewModels);
            }
            catch
            {
                TempData["ErrorMessage"] = "Görevler yüklenirken bir hata oluştu.";
                // Profil var ama teknik hata varsa boş liste dön
                return View(new List<DriverTripTaskViewModel>());
            }
        }

        // ============================================================
        // 2. DETAY SAYFASI
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var taskDto = await _tripTaskApiService.GetByIdAsync(id);

                if (taskDto == null)
                {
                    return NotFound();
                }

                // DTO -> ViewModel Dönüşümü
                var viewModel = taskDto.ToDriverViewModel();

                return View(viewModel);
            }
            catch
            {
                TempData["ErrorMessage"] = "Görev detayları alınamadı.";
                return RedirectToAction(nameof(Index));
            }
        }

        // ============================================================
        // 3. AKSİYONLAR (AJAX)
        // ============================================================

        [HttpPost]
        public async Task<IActionResult> Accept(Guid id)
        {
            var result = await _tripTaskApiService.AcceptTripAsync(id);
            if (result.Succeeded) return Json(new { success = true, message = "Görev kabul edildi." });
            return BadRequest(new { success = false, message = result.Message });
        }

        [HttpPost]
        public async Task<IActionResult> Start(Guid id)
        {
            var result = await _tripTaskApiService.StartTripAsync(id);
            if (result.Succeeded) return Json(new { success = true, message = "Sefer başlatıldı. İyi yolculuklar!" });
            return BadRequest(new { success = false, message = result.Message });
        }

        [HttpPost]
        public async Task<IActionResult> Reject(Guid id, [FromBody] RejectTripRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Reason))
                return BadRequest(new { success = false, message = "Reddetme nedeni zorunludur." });

            var result = await _tripTaskApiService.RejectTripAsync(id, dto);
            if (result.Succeeded) return Json(new { success = true, message = "Görev reddedildi." });
            return BadRequest(new { success = false, message = result.Message });
        }

        [HttpPost]
        public async Task<IActionResult> Complete(Guid id, [FromBody] CompleteTripRequestDto dto)
        {
            // Validasyon
            if (dto.PassengerCount < 0) return BadRequest(new { success = false, message = "Yolcu sayısı negatif olamaz." });
            if (dto.EndOdometerInput <= 0) return BadRequest(new { success = false, message = "Geçerli bir kilometre giriniz." });

            var result = await _tripTaskApiService.CompleteTripAsync(id, dto);

            if (result.Succeeded)
                return Json(new { success = true, message = "Sefer başarıyla tamamlandı. Geçmiş olsun." });

            return BadRequest(new { success = false, message = result.Message });
        }

        [HttpPost]
        public async Task<IActionResult> Fail(Guid id, [FromBody] FailTripRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Reason))
                return BadRequest(new { success = false, message = "Sorun açıklaması zorunludur." });

            var result = await _tripTaskApiService.FailTripAsync(id, dto);

            if (result.Succeeded)
                return Json(new { success = true, message = "Durum merkeze bildirildi." });

            return BadRequest(new { success = false, message = result.Message });
        }
    }
}