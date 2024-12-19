using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace chat_site_server
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string user, string message, string type, string sentAt)
        {
            // بث الرسالة لجميع المستخدمين المتصلين
            await Clients.All.SendAsync("ReceiveMessage", user, message, type, sentAt);
        }
    }
}
