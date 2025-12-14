using IETT_APP.Application.Dtos;
using IETT_APP.Application.Interfaces;
using IETT_APP.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace IETT_APP.Infrastructure.Services
{
    public class AdminUserService : IAdminUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminUserService(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<(bool Succeeded, string Message)> CreateAdminAsync(AdminUserDto dto)
        {
            // Rol var mı kontrol et
            if (await _roleManager.FindByNameAsync(dto.RoleName) == null)
            {
                var roleResult = await _roleManager.CreateAsync(new IdentityRole(dto.RoleName));
                if (!roleResult.Succeeded)
                    return (false, "Rol oluşturulamadı.");
            }

            // Kullanıcı var mı kontrol et
            var existingUser = await _userManager.FindByNameAsync(dto.Email);
            if (existingUser != null)
            {
                return (false, "Bu kullanıcı adı zaten mevcut.");
            }

            var user = new User
            {
                Name = dto.Name,
                Surname = dto.Surname,
                Email = dto.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                var errorMsg = string.Join(", ", result.Errors.Select(e => e.Description));
                return (false, $"Kullanıcı oluşturulamadı: {errorMsg}");
            }

            await _userManager.AddToRoleAsync(user, dto.RoleName);
            return (true, "Admin başarıyla oluşturuldu.");
        }
    }
}
