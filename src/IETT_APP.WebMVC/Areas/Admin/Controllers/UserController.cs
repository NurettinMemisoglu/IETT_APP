using IETT_APP.Application.Dtos;
using IETT_APP.WebMVC.Areas.Admin.Models;
using IETT_APP.WebMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IETT_APP.WebMVC.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class UserController : Controller
    {
        private readonly IApiUserService _userService;
        private readonly IUserSessionApiService _sessionService;

        public UserController(IApiUserService userService, IUserSessionApiService sessionService)
        {
            _userService = userService;
            _sessionService = sessionService;
        }

        // GET: Admin/User
        public async Task<IActionResult> Index()
        {
            // 1. Veriyi API'den DTO olarak çek
            var userDtos = await _userService.GetAllAsync();
            var roles = await _userService.GetAllRolesAsync();

            // 2. DTO -> ViewModel Dönüşümü (Manuel Mapping)
            // Kurumsal projelerde burası için AutoMapper da kullanılabilir ama manuel kontrol daha yüksektir.
            var userViewModels = userDtos.Select(u => new UserUpdateViewModel
            {
                Id = u.Id,
                Name = u.Name,
                Surname = u.Surname,
                Email = u.Email,
                RoleNames = u.RoleNames // Roller sadece gösterim için
            }).ToList();

            // 3. View'a ViewModel gönder
            var viewModel = new UserViewModel
            {
                Users = userViewModels,
                Roles = roles
            };

            return View(viewModel);
        }

        // POST: Admin/User/CreateUser
        [HttpPost]
        public async Task<IActionResult> CreateUser(UserDto user, string password)
        {
            // Basit validasyon (Detaylısı API'de var)
            if (string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(password))
            {
                TempData["ErrorMessage"] = "Email ve şifre zorunludur.";
                return RedirectToAction("Index");
            }

            var result = await _userService.CreateUserAsync(user, password);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Kullanıcı ({user.Email}) başarıyla oluşturuldu.";
            }
            else
            {
                TempData["ErrorMessage"] = "Hata: " + FormatError(result.Errors);
            }

            return RedirectToAction("Index");
        }

        // POST: Admin/User/Update
        [HttpPost]
        public async Task<IActionResult> Update(UserUpdateViewModel model) // Parametre artık ViewModel
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Girdiğiniz bilgiler hatalı.";
                return RedirectToAction("Index");
            }

            // 1. ViewModel -> DTO Dönüşümü
            // API sadece DTO anlar, ViewModel bilmez.
            var userDto = new UserDto
            {
                Id = model.Id,
                Name = model.Name,
                Surname = model.Surname,
                Email = model.Email,
                // RoleNames güncelleme sırasında gönderilmiyor, API o kısmı ellemiyor
            };

            // 2. Servise DTO gönder
            var result = await _userService.UpdateUserAsync(userDto);

            if (result.Succeeded)
                TempData["SuccessMessage"] = "Kullanıcı bilgileri güncellendi.";
            else
            {
                var errorMsg = result.Errors != null && result.Errors.Any()
                    ? string.Join(", ", result.Errors)
                    : "Bilinmeyen hata.";
                TempData["ErrorMessage"] = "Güncelleme hatası: " + errorMsg;
            }

            return RedirectToAction("Index");
        }

        // POST: Admin/User/Delete
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _userService.DeleteUserAsync(id);

            if (result.Succeeded)
                TempData["SuccessMessage"] = "Kullanıcı başarıyla silindi.";
            else
                TempData["ErrorMessage"] = "Silme hatası: " + FormatError(result.Errors);

            return RedirectToAction("Index");
        }

        // POST: Admin/User/AddRole
        [HttpPost]
        public async Task<IActionResult> AddRole(string userId, string roleName)
        {
            var result = await _userService.AddRoleAsync(userId, roleName);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Rol ({roleName}) kullanıcıya eklendi.";
                await _sessionService.RefreshSessionIfSelfAsync(userId);
            }
            else
                TempData["ErrorMessage"] = "Rol ekleme hatası: " + FormatError(result.Errors);

            return RedirectToAction("Index");
        }

        // POST: Admin/User/RemoveRole
        [HttpPost]
        public async Task<IActionResult> RemoveRole(string userId, string roleName)
        {
            var result = await _userService.RemoveRoleAsync(userId, roleName);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Rol kullanıcıdan kaldırıldı.";
                await _sessionService.RefreshSessionIfSelfAsync(userId);
            }
            else
                TempData["ErrorMessage"] = "Rol kaldırma hatası: " + FormatError(result.Errors);

            return RedirectToAction("Index");
        }

        // POST: Admin/User/CreateRole (Yeni Rol Tanımlama)
        [HttpPost]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                TempData["ErrorMessage"] = "Rol adı boş olamaz.";
                return RedirectToAction("Index");
            }

            var result = await _userService.CreateRoleAsync(roleName);

            if (result.Succeeded)
                TempData["SuccessMessage"] = "Yeni rol oluşturuldu.";
            else
                TempData["ErrorMessage"] = "Rol oluşturma hatası: " + FormatError(result.Errors);

            return RedirectToAction("Index");
        }

        // POST: Admin/User/DeleteRole (Rolü Sistemden Silme)
        [HttpPost]
        public async Task<IActionResult> DeleteRole(string roleId) // roleId aslında roleName olabilir
        {
            var result = await _userService.DeleteRoleAsync(roleId);

            if (result.Succeeded)
                TempData["SuccessMessage"] = "Rol sistemden silindi.";
            else
                TempData["ErrorMessage"] = "Rol silme hatası: " + FormatError(result.Errors);

            return RedirectToAction("Index");
        }

        // Yardımcı: Hata listesini string'e çevirir
        private string FormatError(List<string>? errors)
        {
            if (errors == null || !errors.Any()) return "Bilinmeyen hata.";
            return string.Join(", ", errors);
        }
    }
}