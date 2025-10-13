namespace IETT_APP.Application.Dtos
{
    public class StopDto
    {
        public string Id { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // enum adını string olarak döner
        public LocationDto Location { get; set; } = null!;
    }
}