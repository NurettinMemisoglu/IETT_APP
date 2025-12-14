using IETT_APP.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace IETT_APP.Domain.Entities
{
    public class Notification : BaseEntity<Guid>
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        // Bildirim Tipi (Info, Warning, TaskAssignment)
        public string Type { get; set; } = "Info";

        // Okundu mu?
        public bool IsRead { get; set; } = false;

        // Opsiyonel: Tıklayınca gidilecek URL
        public string? LinkUrl { get; set; }

        // Kime Gidecek? (User ID)
        public string UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
    }
}