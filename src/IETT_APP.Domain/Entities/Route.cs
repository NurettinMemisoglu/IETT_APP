using IETT_APP.Domain.Common;
using IETT_APP.Domain.Enums;

namespace IETT_APP.Domain.Entities
{
    public class Route<T> : BaseEntity<T>
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public RoutesDirection RoutesDirection { get; set; }
        public int LengthInM { get; set; }
        public int TimeInMinutes { get; set; }
        public T LineId { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<RouteStop<T>> RouteStops { get; set; } = new List<RouteStop<T>>();
        // NEW: navigation to parent Line
        public Line<T> Line { get; set; } = null!;
    }
}
