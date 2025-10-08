using IETT_APP.Application.Dtos;

namespace IETT_APP.Applicaton.Interfaces
{
    public interface IAdminUserService
    {
        Task<(bool Succeeded, string Message)> CreateAdminAsync(AdminUserDto dto);
    }
}
