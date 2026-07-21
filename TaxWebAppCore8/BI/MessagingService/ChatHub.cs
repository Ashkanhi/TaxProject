using TaxCollectData.Library.Abstraction.Clients;
using Microsoft.AspNetCore.SignalR;
namespace TaxWebAppCore8.BI.MessagingService
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
