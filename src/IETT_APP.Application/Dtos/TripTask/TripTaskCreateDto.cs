using IETT_APP.Domain.Enums;

namespace IETT_APP.Application.Dtos.TripTask
{
    public class TripTaskCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TaskState Status { get; set; } = TaskState.Pending;

        public int? PassengerCount { get; set; }
        public int? DelayInMinutes { get; set; }
        public int? DelayOutMinutes { get; set; }

        public DateTime? ScheduledDeparture { get; set; }
        public DateTime? ScheduledArrival { get; set; }

        public Guid? VehicleId { get; set; }
        public Guid? OperatorId { get; set; }
        public Guid? LineId { get; set; }
        public Guid? RouteId { get; set; }
        public Guid? GarageId { get; set; }
    }

}
