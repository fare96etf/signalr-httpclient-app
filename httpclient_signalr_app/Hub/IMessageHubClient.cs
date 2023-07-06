namespace httpclient_signalr_app.Hub
{
    public interface IMessageHubClient
    {
        Task InformAboutUpdatedBanks(string message);

        Task InformAboutUpdatedCreditCards(string message);
    }
}
