using IETT_APP.Application.Dtos.TripTask;
using IETT_APP.WebMVC.Areas.Driver.Extensions;
using IETT_APP.WebMVC.Areas.Driver.Models;
using IETT_APP.WebMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IETT_APP.WebMVC.Areas.Driver.Controllers
{
    [Area("Driver")]
    [Authorize(Roles = "Driver")]
    public class TasksController : Controller
    {
        private readonly ITripTaskApiService _taskService;

        public TasksController(ITripTaskApiService taskService)
        {
            _taskService = taskService;
        }

        // ============================================================
        // 1. GÖREV LİSTESİ (SAYFA)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                // API'den sadece bu şoföre ait görevleri çek
                var taskDtos = await _taskService.GetMyTasksAsync();

                // DTO -> ViewModel Dönüşümü (Extension Method ile)
                var viewModels = taskDtos
                    .OrderBy(t => t.ScheduledDeparture) // En yakın tarihli görev en üstte
                    .Select(t => t.ToDriverViewModel())
                    .ToList();

                return View(viewModels);
            }
            catch
            {
                TempData["ErrorMessage"] = "Görevler yüklenirken bir hata oluştu.";
                return View(new List<DriverTripTaskViewModel>());
            }
        }

        // ============================================================
        // DETAY SAYFASI
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                // 1. Veriyi API'den çek
                var taskDto = await _taskService.GetByIdAsync(id);

                if (taskDto == null)
                {
                    return NotFound();
                }

                // 2. Güvenlik Kontrolü (Opsiyonel ama önerilir):
                // Şoför sadece kendi görevini görebilmeli.
                // Bu kontrolü API'de yapmak en doğrusudur ama burada da basitçe bakabiliriz
                // veya sadece veriyi gösteririz.

                // 3. ViewModel'e Çevir
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
        // 2. OPERASYONEL AKSİYONLAR (AJAX ile Çağrılacak)
        // ============================================================

        // KABUL ET
        [HttpPost]
        public async Task<IActionResult> Accept(Guid id)
        {
            var result = await _taskService.AcceptTripAsync(id);
            if (result.Succeeded) return Ok(new { message = "Görev kabul edildi." });
            return BadRequest(new { message = result.Message });
        }

        // REDDET (Sebep ile)
        [HttpPost]
        public async Task<IActionResult> Reject(Guid id, [FromBody] RejectTripRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Reason))
                return BadRequest(new { message = "Reddetme nedeni zorunludur." });

            var result = await _taskService.RejectTripAsync(id, dto);
            if (result.Succeeded) return Ok(new { message = "Görev reddedildi." });
            return BadRequest(new { message = result.Message });
        }

        // SEFERİ BAŞLAT (Otomatik KM ve Araç Statüsü)
        [HttpPost]
        public async Task<IActionResult> Start(Guid id)
        {
            var result = await _taskService.StartTripAsync(id);
            if (result.Succeeded) return Ok(new { message = "Sefer başlatıldı. İyi yolculuklar!" });
            return BadRequest(new { message = result.Message });
        }

        // SEFERİ BİTİR (Yolcu Sayısı ve KM Girişi ile)
        [HttpPost]
        public async Task<IActionResult> Complete(Guid id, [FromBody] CompleteTripRequestDto dto)
        {
            // Basit Validasyon
            if (dto.PassengerCount < 0) return BadRequest(new { message = "Yolcu sayısı negatif olamaz." });
            if (dto.EndOdometerInput <= 0) return BadRequest(new { message = "Geçerli bir kilometre giriniz." });

            var result = await _taskService.CompleteTripAsync(id, dto);

            if (result.Succeeded)
                return Ok(new { message = "Sefer başarıyla tamamlandı. Geçmiş olsun." });

            return BadRequest(new { message = result.Message });
        }

        // SORUN BİLDİR / YARIM KALDI
        [HttpPost]
        public async Task<IActionResult> Fail(Guid id, [FromBody] FailTripRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Reason))
                return BadRequest(new { message = "Sorun açıklaması zorunludur." });

            var result = await _taskService.FailTripAsync(id, dto);

            if (result.Succeeded)
                return Ok(new { message = "Durum merkeze bildirildi." });

            return BadRequest(new { message = result.Message });
        }
    }
}