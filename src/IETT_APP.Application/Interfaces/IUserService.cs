using IETT_APP.Application.Dtos;
using Microsoft.AspNetCore.Identity;

namespace IETT_APP.Application.Interfaces
{
    public interface IUserService
    {
        // Kullanıcı İşlemleri
        Task<UserDto> CreateUserAsync(UserDto userDto, string password);
        Task<UserDto> UpdateUserAsync(UserDto userDto);
        Task<UserDto?> GetByIdAsync(string id);
        Task<IEnumerable<UserDto>> GetAllAsync();
        Task<IdentityResult> DeleteUserAsync(string id);
        // Rol İşlemleri
        Task<List<string>> GetAllRolesAsync();
        Task AssignRoleToUserAsync(string userId, string roleName);
        Task RemoveRoleFromUserAsync(string userId, string roleName);
        Task CreateRoleAsync(string roleName);
        Task DeleteRoleAsync(string roleName);
    }
}