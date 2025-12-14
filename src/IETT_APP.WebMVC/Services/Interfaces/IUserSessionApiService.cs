namespace IETT_APP.WebMVC.Services.Interfaces
{
    public interface IUserSessionApiService
    {
        // "Verilen ID şu anki kullanıcıysa oturumunu yenile"
        Task RefreshSessionIfSelfAsync(string targetUserId);

        Task UpdateProfileImageClaimAsync(string newRelativePath);
    }
}