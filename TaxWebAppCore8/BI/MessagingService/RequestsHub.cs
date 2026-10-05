using Microsoft.AspNetCore.SignalR;
namespace TaxWebAppCore8.BI.MessagingService
{
    public class RequestsHub : Hub
    {
        public Task JoinOperation(string operationId)
        {
            if (string.IsNullOrWhiteSpace(operationId))
                throw new HubException("شناسه عملیات معتبر نیست.");

            return Groups.AddToGroupAsync(Context.ConnectionId, operationId);
        }

        public Task LeaveOperation(string operationId)
        {
            if (string.IsNullOrWhiteSpace(operationId))
                return Task.CompletedTask;

            return Groups.RemoveFromGroupAsync(Context.ConnectionId, operationId);
        }

        public async Task NotifyClients()
        {
            // Notify all connected clients to trigger the custom event
            await Clients.All.SendAsync("SigingRequests_Performed");
        }
    }
}
