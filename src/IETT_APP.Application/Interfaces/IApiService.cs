using IETT_APP.Application.Dtos;

namespace IETT_APP.Application.Interfaces
{
    public interface IApiService
    {
        Task<AuthResponseDto> LoginAsync(LoginUserDto dto);
        Task<string> RegisterAsync(RegisterUserDto dto);
    }
}
