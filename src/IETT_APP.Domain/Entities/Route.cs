using IETT_APP.Domain.Common;

namespace IETT_APP.Domain.Entities
{
    public class Route<T> : BaseEntity<T>
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public T LineId { get; set; }

        // NEW: navigation to parent Line
        public Line<T> Line { get; set; } = null!;

        public int LengthInKm { get; set; }
        public int timeInMinutes { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<RouteStop<T>> RouteStops { get; set; } = new List<RouteStop<T>>();
    }
}
