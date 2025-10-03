using IETT_APP.Applicaton.Dtos;
using IETT_APP.Applicaton.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IETT_APP.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // sadece giriş yapmış kullanıcılar erişebilir
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetUserId();
            var profile = await _profileService.GetProfileAsync(userId);
            return Ok(profile);
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = GetUserId();
            var success = await _profileService.UpdateProfileAsync(userId, dto);

            if (!success)
                return BadRequest("Profil güncellenemedi.");

            return Ok(new { Message = "Profil başarıyla güncellendi." });
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = GetUserId();
            var success = await _profileService.ChangePasswordAsync(userId, dto);

            if (!success)
                return BadRequest("Şifre değiştirilemedi.");

            return Ok(new { Message = "Şifre başarıyla değiştirildi." });
        }
    }
}
