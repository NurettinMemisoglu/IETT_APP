using IETT_APP.Domain.Enums;

public class CreateStopDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public StopType StopType { get; set; }
    public SmartStop SmartStop { get; set; }
    public LocationDto Location { get; set; } = null!;
}
