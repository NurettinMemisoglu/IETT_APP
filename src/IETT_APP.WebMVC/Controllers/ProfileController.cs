using IETT_APP.WebMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVCProject.ViewModels;

namespace IETT_APP.WebMVC.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IProfileApiService _profileService;

        public ProfileController(IProfileApiService profileService)
        {
            _profileService = profileService;
        }

        // Profil bilgilerini göster
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var result = await _profileService.GetProfileAsync();

            if (!result.IsSuccess || result.Data == null)
            {
                TempData["Error"] = result.Message ?? "Profil bilgileri alınamadı.";
                return View(new ProfileViewModel()); // boş model gönder
            }

            return View(result.Data);
        }

        // Profil güncelleme
        [HttpPost]
        public async Task<IActionResult> Update(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Index", model);

            var result = await _profileService.UpdateProfileAsync(model);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError("", result.Message);
                return View("Index", model);
            }

            TempData["Success"] = "Profil güncellendi.";
            return RedirectToAction("Index");
        }

        // Şifre değiştirme ekranı
        [HttpGet]
        public IActionResult ChangePassword() => View();

        // Şifre değiştirme post
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _profileService.ChangePasswordAsync(model);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError("", result.Message);
                return View(model);
            }

            TempData["Success"] = "Şifre başarıyla değiştirildi.";
            return RedirectToAction("Index");
        }
    }
}
