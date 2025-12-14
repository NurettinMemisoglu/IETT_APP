using IETT_APP.Application.Dtos;
using IETT_APP.Application.Wrappers; // ServiceResult buradan geliyor
using IETT_APP.WebMVC.Areas.Admin.Models; // RoleViewModel için

namespace IETT_APP.WebMVC.Services.Interfaces
{
    public interface IApiUserService
    {
        // Okuma İşlemleri (Veri döner)
        Task<IEnumerable<UserDto>> GetAllAsync();
        Task<UserDto?> GetByIdAsync(string id);
        Task<IEnumerable<RoleViewModel>> GetAllRolesAsync();

        // Yazma İşlemleri (Sonuç döner)

        Task<ServiceResult<UserDto>> CreateUserAsync(UserDto user, string password);
        Task<ServiceResult> UpdateUserAsync(UserDto user);
        Task<ServiceResult> DeleteUserAsync(string id);

        Task<ServiceResult> AddRoleAsync(string userId, string roleName);
        Task<ServiceResult> RemoveRoleAsync(string userId, string roleName);
        Task<ServiceResult> CreateRoleAsync(string roleName);
        Task<ServiceResult> DeleteRoleAsync(string roleName);
    }
}