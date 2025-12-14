using IETT_APP.Domain.Enums;

namespace IETT_APP.Application.Dtos.TripTask
{
    public class UpdateTaskStatusRequest
    {
        public TaskState Status { get; set; }
        public string? Reason { get; set; }
    }
}
