namespace IETT_APP.Application.Dtos
{
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = "Info";
        public bool IsRead { get; set; }
        public string? LinkUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}