namespace IETT_APP.Application.Interfaces
{
    // Frontend (JS) tarafındaki metodun adı ve parametreleri burada belirlenir.
    public interface INotificationClient
    {
        // JS tarafında: connection.on("ReceiveNotification", (message, title, link) => { ... })
        Task ReceiveNotification(string title, string message, string importance, string linkUrl, Guid notificationId);

        // Canlı Güncelleme Sinyali (Tabloyu yeniler)
        // JS tarafında: connection.on("TaskUpdated", (id) => { ... })
        Task TaskUpdated(Guid taskId);
    }
}