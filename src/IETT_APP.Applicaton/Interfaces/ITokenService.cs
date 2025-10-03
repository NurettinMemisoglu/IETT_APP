using IETT_APP.Domain.Entities;

namespace IETT_APP.Applicaton.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(User user, IList<string> roles);
        string GenerateRefreshToken();
    }
}
