using IETT_APP.Domain.Enums;

namespace IETT_APP.Application.Dtos.TripTask
{
    public class TripTaskDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TaskState Status { get; set; }

        public string? StatusReason { get; set; }

        public int? PassengerCount { get; set; }
        public int? DelayInMinutes { get; set; }
        public int? DelayOutMinutes { get; set; }

        public DateTime? ScheduledDeparture { get; set; }
        public DateTime? ScheduledArrival { get; set; }
        public DateTime? AdjustedDeparture { get; set; }
        public DateTime? AdjustedArrival { get; set; }
        public DateTime? ActualDeparture { get; set; }
        public DateTime? ActualArrival { get; set; }

        public Guid? VehicleId { get; set; }
        public Guid? OperatorId { get; set; }
        public Guid? LineId { get; set; }
        public Guid? RouteId { get; set; }
        public Guid? GarageId { get; set; }

        public string? VehicleName { get; set; }
        public string? OperatorName { get; set; }
        public string? LineName { get; set; }
        public string? RouteName { get; set; }
        public string? GarageName { get; set; }

        // Opsiyonel: Read-only alanlar (UI veya log amaçlı)
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
