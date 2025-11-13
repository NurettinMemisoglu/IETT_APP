using IETT_APP.Domain.Common;
using IETT_APP.Domain.Enums;

namespace IETT_APP.Domain.Entities
{
    public class Line<T> : BaseEntity<T>
    {
        public string Code { get; set; } = string.Empty;
        public LineType LineType { get; set; }
        public string Name { get; set; } = string.Empty;
        public int VehicleCount { get; set; }
        public bool IsActive { get; set; } = true;

        // NEW: navigation to routes
        public ICollection<Route<T>> Routes { get; set; } = new List<Route<T>>();
        public ICollection<TripTask> TripTasks { get; set; } = new List<TripTask>();
    }
}
