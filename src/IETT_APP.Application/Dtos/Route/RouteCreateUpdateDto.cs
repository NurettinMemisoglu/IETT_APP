using IETT_APP.Domain.Enums;

namespace IETT_APP.Application.Dtos.Route
{
    public class RouteCreateUpdateDto<T>
    {
        public T? Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int LengthInM { get; set; }
        public int TimeInMinutes { get; set; }
        public RoutesDirection RoutesDirection { get; set; }
        public T LineId { get; set; } = default!;
        public List<Guid> StopIds { get; set; } = new();
        public List<string> StopNames { get; set; } = new();
        public bool IsActive { get; set; }

    }
}
