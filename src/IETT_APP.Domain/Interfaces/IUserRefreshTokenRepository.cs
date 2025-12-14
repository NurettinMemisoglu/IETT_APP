using IETT_APP.Domain.Entities;

namespace IETT_APP.Domain.Interfaces
{
    public interface IUserRefreshTokenRepository
    {
        Task AddAsync(UserRefreshToken token);
        Task<UserRefreshToken?> GetByTokenAsync(string token);
        Task<UserRefreshToken?> GetByUserIdAsync(string userId);
        Task DeleteAsync(UserRefreshToken token);
        Task DeleteAllByUserIdAsync(string userId); // Tüm cihazlardan çıkış için
    }
}