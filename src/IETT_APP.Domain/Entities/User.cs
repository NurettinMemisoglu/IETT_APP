using IETT_APP.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace IETT_APP.Domain.Entities
{
    public class User : IdentityUser, IAuditableEntity
    {
        public string? Name { get; set; }
        public string? Surname { get; set; }

        [NotMapped]
        public IList<string> RoleNames { get; set; } = null!;
        public Driver? Driver { get; set; }

        public bool IsDeleted { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }

    }
}