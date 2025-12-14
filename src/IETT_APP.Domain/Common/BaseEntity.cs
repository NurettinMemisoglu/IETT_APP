using IETT_APP.Domain.Interfaces;

namespace IETT_APP.Domain.Common
{
    public abstract class BaseEntity<T> : IAuditableEntity
    {
        public T Id { get; set; } = default!;

        public bool IsDeleted { get; set; } = false;

        // Audit Fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
