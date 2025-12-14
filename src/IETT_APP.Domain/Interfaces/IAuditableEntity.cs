// IETT_APP.Domain/Interfaces/IAuditableEntity.cs
namespace IETT_APP.Domain.Interfaces
{
    public interface IAuditableEntity
    {
        DateTime CreatedAt { get; set; }
        string? CreatedBy { get; set; }
        DateTime? UpdatedAt { get; set; }
        string? UpdatedBy { get; set; }
        DateTime? DeletedAt { get; set; }
        string? DeletedBy { get; set; }
        bool IsDeleted { get; set; }
        bool IsActive { get; set; }
    }
}
