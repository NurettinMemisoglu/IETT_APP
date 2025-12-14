using IETT_APP.Application.Dtos;
using IETT_APP.Application.Wrappers;

namespace IETT_APP.WebMVC.Services.Interfaces
{
    public interface IAuthApiService
    {
        Task<AuthResponseDto?> RegisterAsync(RegisterUserDto registerDto);
        Task<AuthResponseDto?> LoginAsync(LoginUserDto loginDto);
        Task<ServiceResult> ChangePasswordAsync(ChangePasswordDto dto);
        Task LogoutAsync();
    }
}