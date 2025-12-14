using IETT_APP.Application.Dtos;
using IETT_APP.Application.Interfaces;
using IETT_APP.Application.Wrappers;
using IETT_APP.Domain.Entities;
using IETT_APP.Domain.Interfaces; // Repository Interface burada
using Microsoft.AspNetCore.Identity;

namespace IETT_APP.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IUserRefreshTokenRepository _refreshTokenRepository; // Yeni Repo

        public AuthService(
            UserManager<User> userManager,
            ITokenService tokenService,
            IUserRefreshTokenRepository refreshTokenRepository)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task<AuthResponseDto?> RegisterAsync(RegisterUserDto dto)
        {
            var user = new User
            {
                Email = dto.Email,
                UserName = dto.Email,
                Name = dto.Name,
                Surname = dto.Surname,
                IsActive = true // Varsayılan aktif
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded) return null;

            // HATA DÜZELTME: Rol oluşturma kodu silindi. Seed zaten yapıyor.
            // Sadece atama yapıyoruz. Hata almamak için try-catch eklenebilir.
            await _userManager.AddToRoleAsync(user, "User");

            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _tokenService.GenerateToken(user, roles);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Repository Kullanımı
            await _refreshTokenRepository.AddAsync(new UserRefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiryTime = DateTime.UtcNow.AddDays(7)
            });

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginUserDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                return null;

            // Pasif kullanıcı kontrolü (Kurumsal kural)
            if (!user.IsActive) return null; // Veya özel hata fırlatılabilir

            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _tokenService.GenerateToken(user, roles);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Repository Kullanımı
            await _refreshTokenRepository.AddAsync(new UserRefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiryTime = DateTime.UtcNow.AddDays(7)
            });

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken)
        {
            // Repository Kullanımı
            var tokenEntry = await _refreshTokenRepository.GetByTokenAsync(refreshToken);

            if (tokenEntry == null || tokenEntry.ExpiryTime < DateTime.UtcNow)
                return null;

            var roles = await _userManager.GetRolesAsync(tokenEntry.User);
            var accessToken = _tokenService.GenerateToken(tokenEntry.User, roles);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            // Rotation: Eskiyi sil, yeniyi ekle
            await _refreshTokenRepository.DeleteAsync(tokenEntry);

            await _refreshTokenRepository.AddAsync(new UserRefreshToken
            {
                UserId = tokenEntry.UserId,
                Token = newRefreshToken,
                ExpiryTime = DateTime.UtcNow.AddDays(7)
            });

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken
            };
        }

        public async Task LogoutAsync(string userId)
        {
            // Kullanıcının tüm oturumlarını kapatmak için:
            await _refreshTokenRepository.DeleteAllByUserIdAsync(userId);
        }

        public async Task<ServiceResult> ChangePasswordAsync(string userId, ChangePasswordDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return ServiceResult.Failure("Kullanıcı bulunamadı.");
            }

            var verificationResult = _userManager.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash!, dto.NewPassword);

            if (verificationResult == PasswordVerificationResult.Success)
            {
                return ServiceResult.Failure("Yeni şifreniz eski şifrenizle aynı olamaz.");
            }

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return ServiceResult.Failure(errors);
            }

            return ServiceResult.Success("Şifre başarıyla değiştirildi.");
        }
    }
}