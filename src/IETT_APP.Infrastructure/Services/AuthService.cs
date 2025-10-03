using IETT_APP.Applicaton.Dtos;
using IETT_APP.Applicaton.Interfaces;
using IETT_APP.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IETT_APP.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly ITokenService _tokenService;

        public AuthService(UserManager<User> userManager, RoleManager<Role> roleManager, ITokenService tokenService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
        }

        // Register
        public async Task<AuthResponseDto> RegisterAsync(RegisterUserDto dto)
        {
            var user = new User
            {
                Email = dto.Email,
                FullName = dto.FullName,
                UserName = dto.UserName
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return null; // Hata durumunda null dönebiliriz

            // Role oluştur ve kullanıcıya ekle
            if (!await _roleManager.RoleExistsAsync("User"))
                await _roleManager.CreateAsync(new Role { Name = "User" });

            await _userManager.AddToRoleAsync(user, "User");

            // Token üret
            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _tokenService.GenerateToken(user, roles);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // DB'ye kaydet
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }


        // Login
        public async Task<AuthResponseDto> LoginAsync(LoginUserDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                return null;

            var roles = await _userManager.GetRolesAsync(user);

            var accessToken = _tokenService.GenerateToken(user, roles);
            var refreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken)
        {
            // find user by refresh token
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
            if (user == null || user.RefreshTokenExpiryTime < DateTime.UtcNow) return null;

            // rotate token
            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _tokenService.GenerateToken(user, roles);
            var newRefresh = _tokenService.GenerateRefreshToken();

            user.RefreshToken = newRefresh;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            return new AuthResponseDto { AccessToken = accessToken, RefreshToken = newRefresh };
        }

        public async Task LogoutAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return;
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = DateTime.MinValue;
            await _userManager.UpdateAsync(user);
        }
    }
}
