using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace IETT_APP.Domain.Entities
{
    public class User : IdentityUser
    {
        public string? FullName { get; set; }

        [NotMapped]
        public IList<string> RoleNames { get; set; } = null!;
    }
}