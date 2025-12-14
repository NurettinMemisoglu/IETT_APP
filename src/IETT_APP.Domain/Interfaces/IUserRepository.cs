using IETT_APP.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace IETT_APP.Domain.Interfaces
{
    public interface IUserRepository
    {
        // createdBy parametresi eklendi
        Task<IdentityResult> AddAsync(User user, string password, string? createdBy = null);

        Task<User?> GetByIdAsync(string id);
        Task<IEnumerable<User>> GetAllAsync(); // Sadece silinmemişleri getirir

        Task<IdentityResult> UpdateAsync(User user, string? updatedBy = null);
        Task<IdentityResult> SoftDeleteAsync(string id, string? deletedBy = null);

        Task<IList<string>> GetUserRolesAsync(User user);

        // Hata kontrolü yapabilmek için void yerine IdentityResult dönmeli
        Task<IdentityResult> AddToRoleAsync(User user, string role);
    }
}