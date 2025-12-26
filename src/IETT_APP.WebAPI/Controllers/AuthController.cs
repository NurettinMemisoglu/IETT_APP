using IETT_APP.Application.Dtos;
using IETT_APP.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IETT_APP.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterUserDto dto)
        {
            try
            {
                // Artık Service hata fırlattığı için burası başarılıysa kesin veri vardır.
                var authResult = await _authService.RegisterAsync(dto);
                return Ok(authResult); // 200 OK
            }
            catch (Exception ex)
            {
                // Service'ten gelen "Şifreler uyuşmuyor" veya "Email zaten var"
                // hatası burada yakalanır ve 400 olarak döner.
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginUserDto dto)
        {
            var authResult = await _authService.LoginAsync(dto);
            if (authResult == null)
                return Unauthorized(new { message = "Email veya şifre hatalı." });

            return Ok(authResult);
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh([FromBody] string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
                return BadRequest(new { message = "Refresh token boş olamaz." });

            var result = await _authService.RefreshTokenAsync(refreshToken);
            if (result == null)
                return Unauthorized(new { message = "Refresh token geçersiz veya süresi dolmuş." });

            return Ok(result);
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Kullanıcı bulunamadı." });

            await _authService.LogoutAsync(userId);
            return Ok(new { message = "Çıkış yapıldı." });
        }

        // POST: api/auth/change-password
        [HttpPost("change-password")]
        [Authorize] // Token ZORUNLU
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Token içindeki User ID'yi alıyoruz (Güvenlik için)
            // Body'den ID almak yerine Token'dan almak başkasının şifresini değiştirmeyi engeller.
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId)) return Unauthorized("Kullanıcı kimliği doğrulanamadı.");

            var result = await _authService.ChangePasswordAsync(userId, dto);

            if (result.Succeeded)
            {
                return Ok(new { message = result.Message });
            }

            return BadRequest(new { message = "Hata oluştu", errors = result.Errors });
        }
    }
}
