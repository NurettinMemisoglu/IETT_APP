using IETT_APP.Domain.Entities;
using IETT_APP.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IETT_APP.Infrastructure.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<User> _userManager;

        public UserRepository(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IdentityResult> AddAsync(User user, string password, string? createdBy = null)
        {
            // Interceptor: CreatedAt, UpdatedAt tarihlerini set edecek.

            user.IsActive = true;
            user.IsDeleted = false;

            // Eğer servis createdBy göndermezse "System" kullan
            user.CreatedBy = createdBy ?? "System";

            return await _userManager.CreateAsync(user, password);
        }

        public async Task<User?> GetByIdAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            // Kullanıcı yoksa veya Silinmişse null dön
            if (user == null || user.IsDeleted) return null;

            return user;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            // Sadece aktif (silinmemiş) kullanıcılar
            return await _userManager.Users
                                     .Where(u => !u.IsDeleted)
                                     .ToListAsync();
        }

        public async Task<IdentityResult> UpdateAsync(User user, string? updatedBy = null)
        {
            // UpdatedAt interceptor tarafından set edilecek.
            user.UpdatedBy = updatedBy;

            return await _userManager.UpdateAsync(user);
        }

        public async Task<IdentityResult> SoftDeleteAsync(string id, string? deletedBy = null)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "Kullanıcı bulunamadı." });

            if (user.IsDeleted)
                return IdentityResult.Failed(new IdentityError { Description = "Kullanıcı zaten silinmiş." });

            // Sadece silen kişiyi set ediyoruz.
            user.DeletedBy = deletedBy;

            // DeleteAsync çağırıyoruz FAKAT AuditInterceptor bunu yakalayıp 
            // state'i Modified yapacak ve IsDeleted=true set edecek.
            return await _userManager.DeleteAsync(user);
        }

        public async Task<IList<string>> GetUserRolesAsync(User user)
        {
            return await _userManager.GetRolesAsync(user);
        }

        public async Task<IdentityResult> AddToRoleAsync(User user, string role)
        {
            return await _userManager.AddToRoleAsync(user, role);
        }
    }
}