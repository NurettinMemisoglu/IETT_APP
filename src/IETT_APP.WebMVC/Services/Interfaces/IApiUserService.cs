using IETT_APP.Application.Dtos;
using Microsoft.AspNetCore.Identity;

namespace IETT_APP.WebMVC.Services.Interfaces
{
    public interface IApiUserService
    {
        Task<IEnumerable<UserDto>> GetAllAsync();
        Task<UserDto?> GetByIdAsync(string id);
        Task<(IdentityResult Result, UserDto? User)> CreateUserAsync(UserDto user, string password);
        Task<IdentityResult> DeleteUserAsync(string id);
        Task<IdentityResult> AddRoleAsync(string userId, string roleName);
        Task<IdentityResult> RemoveRoleAsync(string userId, string roleName);
        Task<IdentityResult> CreateRoleAsync(string roleName);
        Task<IdentityResult> DeleteRoleAsync(string roleId);
        Task<IEnumerable<IdentityRole>> GetRolesAsync();
    }
}
