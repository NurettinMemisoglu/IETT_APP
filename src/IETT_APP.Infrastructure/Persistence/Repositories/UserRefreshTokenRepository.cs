using IETT_APP.Domain.Entities;
using IETT_APP.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IETT_APP.Infrastructure.Persistence.Repositories
{
    public class UserRefreshTokenRepository : IUserRefreshTokenRepository
    {
        private readonly AppDbContext _context;

        public UserRefreshTokenRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(UserRefreshToken token)
        {
            await _context.UserRefreshTokens.AddAsync(token);
            await _context.SaveChangesAsync();
        }

        public async Task<UserRefreshToken?> GetByTokenAsync(string token)
        {
            // User bilgisini de Include ediyoruz (RefreshTokenAsync'de lazım)
            return await _context.UserRefreshTokens
                                 .Include(t => t.User)
                                 .FirstOrDefaultAsync(t => t.Token == token);
        }

        public async Task<UserRefreshToken?> GetByUserIdAsync(string userId)
        {
            return await _context.UserRefreshTokens.FirstOrDefaultAsync(t => t.UserId == userId);
        }

        public async Task DeleteAsync(UserRefreshToken token)
        {
            _context.UserRefreshTokens.Remove(token);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAllByUserIdAsync(string userId)
        {
            var tokens = _context.UserRefreshTokens.Where(t => t.UserId == userId);
            _context.UserRefreshTokens.RemoveRange(tokens);
            await _context.SaveChangesAsync();
        }
    }
}