using IETT_APP.Applicaton.Dtos;

namespace IETT_APP.Applicaton.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> RegisterAsync(RegisterUserDto dto);
        Task<AuthResponseDto?> LoginAsync(LoginUserDto dto);
        Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken);
        Task LogoutAsync(string userId);
    }
}
