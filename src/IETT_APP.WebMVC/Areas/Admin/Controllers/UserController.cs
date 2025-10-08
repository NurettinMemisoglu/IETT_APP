using IETT_APP.WebMVC.Areas.Admin.Models;
using IETT_APP.WebMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IETT_APP.WebMVC.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class UserController : Controller
    {
        private readonly IApiUserService _userService; // artık API üzerinden
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(IApiUserService userService, RoleManager<IdentityRole> roleManager)
        {
            _userService = userService;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var userDto = await _userService.GetAllAsync();

            // DB'deki tüm roller
            var identityRoles = _roleManager.Roles.ToList();

            var userViewModel = new UserViewModel
            {
                Users = userDto,
                Roles = identityRoles
            };

            return View(userViewModel);
        }


        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Kullanıcı başarıyla silindi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Kullanıcı silinirken bir hata oluştu: " +
                                           string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AddRole(string userId, string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                TempData["ErrorMessage"] = "Rol adı boş olamaz.";
                return RedirectToAction("Index");
            }

            var result = await _userService.AddRoleAsync(userId, roleName);

            if (result.Succeeded)
                TempData["SuccessMessage"] = "Rol başarıyla eklendi.";
            else
                TempData["ErrorMessage"] = "Rol eklenirken hata oluştu: " + string.Join(", ", result.Errors.Select(e => e.Description));

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveRole(string userId, string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                TempData["ErrorMessage"] = "Rol adı boş olamaz.";
                return RedirectToAction("Index");
            }

            var result = await _userService.RemoveRoleAsync(userId, roleName);

            if (result.Succeeded)
                TempData["SuccessMessage"] = "Rol başarıyla kaldırıldı.";
            else
                TempData["ErrorMessage"] = "Rol kaldırılırken hata oluştu: " + string.Join(", ", result.Errors.Select(e => e.Description));

            return RedirectToAction("Index");
        }

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
            {
                TempData["SuccessMessage"] = "Rol başarıyla oluşturuldu.";
            }
            else
            {
                TempData["ErrorMessage"] = "Rol oluşturulurken hata oluştu: " +
                                           string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRole(string roleId)
        {
            if (string.IsNullOrEmpty(roleId))
            {
                TempData["ErrorMessage"] = "Rol ID geçersiz.";
                return RedirectToAction("Index");
            }

            var result = await _userService.DeleteRoleAsync(roleId);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Rol başarıyla silindi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Rol silinirken hata oluştu: " +
                                           string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction("Index");
        }

    }
}
