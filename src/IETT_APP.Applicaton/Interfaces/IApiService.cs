using IETT_APP.Applicaton.Dtos;

namespace IETT_APP.Applicaton.Interfaces
{
    public interface IApiService
    {
        Task<AuthResponseDto> LoginAsync(LoginUserDto dto);
        Task<string> RegisterAsync(RegisterUserDto dto);
    }
}
