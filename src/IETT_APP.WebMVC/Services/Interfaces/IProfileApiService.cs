using MVCProject.ViewModels;

namespace IETT_APP.WebMVC.Services.Interfaces
{
    public interface IProfileApiService

    {
        Task<(bool IsSuccess, string Message, ProfileViewModel? Data)> GetProfileAsync();
        Task<(bool IsSuccess, string Message)> UpdateProfileAsync(ProfileViewModel model);
        Task<(bool IsSuccess, string Message)> ChangePasswordAsync(ChangePasswordViewModel model);
    }
}
