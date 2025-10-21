using IETT_APP.Application.Dtos;

namespace IETT_APP.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> CreateUserAsync(UserDto userDto, string password);
        Task<UserDto?> GetByIdAsync(string id);
        Task<IEnumerable<UserDto>> GetAllAsync();
        Task DeleteUserAsync(string id);

    }
}
