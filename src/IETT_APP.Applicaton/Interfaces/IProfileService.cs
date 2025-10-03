using IETT_APP.Applicaton.Dtos;

namespace IETT_APP.Applicaton.Interfaces
{
    public interface IProfileService
    {
        Task<ProfileDto> GetProfileAsync(string userId);
        Task<bool> UpdateProfileAsync(string userId, UpdateProfileDto dto);
        Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto dto);
    }
}
