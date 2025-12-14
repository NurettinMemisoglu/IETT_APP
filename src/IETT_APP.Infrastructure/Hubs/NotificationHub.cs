using IETT_APP.Application.Interfaces; // INotificationClient burada
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace IETT_APP.Infrastructure.Hubs
{
    [Authorize] // Sadece giriş yapmış kullanıcılar dinleyebilir
    // Hub<T> kullanarak tip güvenli hale getirdik.
    public class NotificationHub : Hub<INotificationClient>
    {
        // Burası şimdilik boş kalabilir. 
        // Bağlantı olaylarını loglamak istersen override edebilirsin.
        public override async Task OnConnectedAsync()
        {
            // Kullanıcı bağlandığında UserID'si ile otomatik grup oluşur.
            await base.OnConnectedAsync();
        }
    }
}