using IETT_APP.Applicaton.Dtos;
using IETT_APP.Applicaton.Interfaces;
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
            var authResult = await _authService.RegisterAsync(dto);
            if (authResult == null)
                return BadRequest(new { message = "Kayıt sırasında hata oluştu." });

            return Ok(authResult);
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
    }
}
