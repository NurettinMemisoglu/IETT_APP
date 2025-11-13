using IETT_APP.Domain.Common;

namespace IETT_APP.Domain.Entities
{
    public class Operator : BaseEntity<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public string EmployeeNumber { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        // İlişkiler
        public ICollection<TripTask> TripTasks { get; set; } = new List<TripTask>();
    }

}
