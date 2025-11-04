using IETT_APP.Domain.Enums;

public class StopInfoDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public int Order { get; set; }
}

public class RouteDto<T>
{
    public T Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int LengthInM { get; set; }
    public int TimeInMinutes { get; set; }
    public RoutesDirection RoutesDirection { get; set; }
    public T LineId { get; set; }

    // Durak bilgileri
    public List<Guid> StopIds { get; set; } = new();
    public List<string> StopNames { get; set; } = new();
    public List<StopInfoDto> Stops { get; set; } = new(); // yeni eklendi

    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }  // nullable olmalı
}
