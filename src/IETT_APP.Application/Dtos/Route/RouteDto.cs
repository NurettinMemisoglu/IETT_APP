using IETT_APP.Domain.Enums;

namespace IETT_APP.Application.Dtos.Route
{
    public class RouteDto<T>
    {
        public T Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int LengthInM { get; set; }
        public int TimeInMinutes { get; set; }
        public RouteDirection RouteDirection { get; set; }
        public T LineId { get; set; }
        public List<Guid> StopIds { get; set; } = new();
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }  // nullable olmalı
    }
}
