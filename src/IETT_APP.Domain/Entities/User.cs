using Microsoft.AspNetCore.Identity;

namespace IETT_APP.Domain.Entities
{
    public class User : IdentityUser
    {
        public string? FullName { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }

    }
}
