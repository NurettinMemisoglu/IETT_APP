namespace IETT_APP.Domain.Entities
{
    public class TripTaskHistory
    {
        public Guid Id { get; set; }
        public Guid TripTaskId { get; set; }

        public string FieldName { get; set; } = string.Empty;
        public string OldValue { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
