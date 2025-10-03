using Microsoft.AspNetCore.Identity;

namespace IETT_APP.Domain.Entities
{
    public class Role : IdentityRole
    {
        public string? Description { get; set; } // Ek alan
    }
}
