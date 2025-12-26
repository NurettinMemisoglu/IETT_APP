using IETT_APP.Application.Dtos;
using IETT_APP.Application.Wrappers; // Bunu eklemeyi unutma

namespace IETT_APP.WebMVC.Services.Interfaces
{
    public interface IAuthApiService
    {
        Task<AuthResponseDto?> LoginAsync(LoginUserDto loginDto);

        Task<ServiceResult<AuthResponseDto>> RegisterAsync(RegisterUserDto registerDto);

        Task<ServiceResult> ChangePasswordAsync(ChangePasswordDto dto);
        Task LogoutAsync();
    }
}