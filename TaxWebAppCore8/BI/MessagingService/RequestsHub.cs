using Microsoft.AspNetCore.SignalR;
namespace TaxWebAppCore8.BI.MessagingService
{
    public class RequestsHub : Hub
    {
        public async Task NotifyClients()
        {
            // Notify all connected clients to trigger the custom event
            await Clients.All.SendAsync("SigingRequests_Performed");
        }
    }
}
