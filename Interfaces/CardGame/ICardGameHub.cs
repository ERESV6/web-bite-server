using SignalRSwaggerGen.Attributes;
using web_bite_server.Dtos.CardGame;

namespace web_bite_server.interfaces.CardGame
{

    [SignalRHub("")]
    public interface ICardGameHub
    {
        Task UserConnection(string userName);
        Task UserConnections(List<CardGameActiveUserDto> activeUsers);
        Task RequestCardGameConnection(string userName);
        Task AcceptCardGameConnection();
        Task DeclineCardGameConnection();
        Task PlayCard(int playedCardsNumber);
    }
}