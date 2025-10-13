namespace IETT_APP.Applicaton.Dtos.Stop
{
    public class UpdateStopDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public LocationDto Location { get; set; } = null!;
    }
}
