using Microsoft.AspNetCore.SignalR;

namespace httpclient_signalr_app.Hub
{
    public class MessageHub : Hub<IMessageHubClient>
    {
        public async Task InformAboutUpdatedBanks(string message)
        {
            await Clients.All.InformAboutUpdatedBanks(message);
        }

        public async Task InformAboutUpdatedCreditCards(string message)
        {
            await Clients.All.InformAboutUpdatedCreditCards(message);
        }
    }
}
