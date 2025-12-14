using IETT_APP.Application.Dtos;
using IETT_APP.Application.Dtos.IETT_APP.Application.Dtos;
using IETT_APP.Application.Interfaces;
using IETT_APP.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace IETT_APP.Infrastructure.Services
{
    public class ProfileService : IProfileService
    {
        private readonly UserManager<User> _userManager;

        public ProfileService(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ProfileDto> GetProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new Exception("Kullanıcı bulunamadı.");

            return new ProfileDto
            {
                Email = user.Email ?? "",
                Name = user.Name ?? "",
                Surname = user.Surname ?? ""
            };
        }

        public async Task<bool> UpdateProfileAsync(string userId, UpdateProfileDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            user.Email = dto.Email;
            if (!string.IsNullOrWhiteSpace(dto.Name))
                user.Name = dto.Name;

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            return result.Succeeded;
        }
    }
}