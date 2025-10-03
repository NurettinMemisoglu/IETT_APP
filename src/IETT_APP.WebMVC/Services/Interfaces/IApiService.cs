using MVCProject.ViewModels;

namespace MVCProject.Services.Interfaces
{
    public interface IApiService
    {
        Task<(bool IsSuccess, string Message)> RegisterAsync(RegisterViewModel model);
        Task<(bool IsSuccess, string Message, string Token)> LoginAsync(LoginViewModel model);

        // Token yönetimi
        void SetTokenHeader(string token);
        void RemoveTokenHeader();

        // Session'dan veya saklanan yerden token al
        string GetToken();
    }
}
