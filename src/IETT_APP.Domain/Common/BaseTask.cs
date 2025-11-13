
using IETT_APP.Domain.Enums;

namespace IETT_APP.Domain.Common
{
    public abstract class BaseTask<TKey> : BaseEntity<TKey>
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TaskState Status { get; set; } = TaskState.Pending;
        public DateTime? CompletedAt { get; set; }
    }

}
