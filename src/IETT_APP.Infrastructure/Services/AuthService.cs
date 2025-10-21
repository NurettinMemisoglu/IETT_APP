using IETT_APP.Application.Dtos;
using IETT_APP.Application.Interfaces;
using IETT_APP.Domain.Entities;
using IETT_APP.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IETT_APP.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ITokenService _tokenService;
        private readonly AppDbContext _context;

        public AuthService(UserManager<User> userManager, RoleManager<IdentityRole> roleManager,
            ITokenService tokenService, AppDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
            _context = context;
        }

        public async Task<AuthResponseDto?> RegisterAsync(RegisterUserDto dto)
        {
            var user = new User
            {
                Email = dto.Email,
                // Use email as username to remove username from UI concerns
                UserName = dto.Email,
                FullName = dto.FullName
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded) return null;

            if (!await _roleManager.RoleExistsAsync("User"))
                await _roleManager.CreateAsync(new IdentityRole { Name = "User" });
            await _userManager.AddToRoleAsync(user, "User");

            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _tokenService.GenerateToken(user, roles);

            var refreshToken = _tokenService.GenerateRefreshToken();
            _context.UserRefreshTokens.Add(new UserRefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiryTime = DateTime.UtcNow.AddDays(7)
            });
            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        // Login unchanged (it already finds by email)
        public async Task<AuthResponseDto?> LoginAsync(LoginUserDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                return null;

            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _tokenService.GenerateToken(user, roles);

            var refreshToken = _tokenService.GenerateRefreshToken();
            _context.UserRefreshTokens.Add(new UserRefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiryTime = DateTime.UtcNow.AddDays(7)
            });
            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken)
        {
            var tokenEntry = await _context.UserRefreshTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == refreshToken);

            if (tokenEntry == null || tokenEntry.ExpiryTime < DateTime.UtcNow)
                return null;

            var roles = await _userManager.GetRolesAsync(tokenEntry.User);
            var accessToken = _tokenService.GenerateToken(tokenEntry.User, roles);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            _context.UserRefreshTokens.Remove(tokenEntry);
            _context.UserRefreshTokens.Add(new UserRefreshToken
            {
                UserId = tokenEntry.UserId,
                Token = newRefreshToken,
                ExpiryTime = DateTime.UtcNow.AddDays(7)
            });
            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken
            };
        }

        public async Task LogoutAsync(string userId)
        {
            var tokens = _context.UserRefreshTokens.Where(t => t.UserId == userId);
            _context.UserRefreshTokens.RemoveRange(tokens);
            await _context.SaveChangesAsync();
        }
    }
}
