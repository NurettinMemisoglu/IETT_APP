using IETT_APP.Application.Dtos;

namespace IETT_APP.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> RegisterAsync(RegisterUserDto dto);
        Task<AuthResponseDto?> LoginAsync(LoginUserDto dto);
        Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken);
        Task LogoutAsync(string userId);
    }
}
