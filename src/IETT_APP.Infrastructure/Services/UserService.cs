// IETT_APP.Infrastructure/Services/UserService.cs
using IETT_APP.Application.Dtos;
using IETT_APP.Applicaton.Interfaces;
using IETT_APP.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IETT_APP.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserService(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Yeni kullanıcı oluştur
        public async Task<UserDto> CreateUserAsync(UserDto userDto, string password)
        {
            var user = new User
            {
                UserName = userDto.Email,
                FullName = userDto.FullName,
                Email = userDto.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Kullanıcı oluşturulamadı: {errors}");
            }

            // Roller varsa ekle
            if (userDto.RoleNames != null && userDto.RoleNames.Count > 0)
            {
                foreach (var role in userDto.RoleNames)
                {
                    if (!await _roleManager.RoleExistsAsync(role))
                        await _roleManager.CreateAsync(new IdentityRole(role));

                    await _userManager.AddToRoleAsync(user, role);
                }
            }

            userDto.Id = user.Id;
            return userDto;
        }

        // Id’ye göre kullanıcı getir
        public async Task<UserDto?> GetByIdAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);

            return new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                RoleNames = roles.ToList()
            };
        }

        // Tüm kullanıcıları getir
        public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var result = new List<UserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                result.Add(new UserDto
                {
                    Id = user.Id,
                    UserName = user.Email,
                    FullName = user.FullName,
                    Email = user.Email,
                    RoleNames = roles.ToList()
                });
            }

            return result;
        }

        // Kullanıcı sil
        public async Task DeleteUserAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }
        }
    }
}
