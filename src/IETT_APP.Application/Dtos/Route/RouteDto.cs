namespace IETT_APP.Application.Dtos.Route
{
    public class RouteDto<T>
    {
        public T Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public T LineId { get; set; }
        public int LengthInKm { get; set; }
        public int timeInMinutes { get; set; }
        public bool IsDeleted { get; set; } = false;
        public bool IsActive { get; set; } = true;

        public List<Guid> StopIds { get; set; } = new();
    }
}
