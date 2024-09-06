using SignalRSwaggerGen.Attributes;
using web_bite_server.Dtos.CardGame;

namespace web_bite_server.interfaces.CardGame
{

    [SignalRHub("")]
    public interface IUsersHub
    {
        Task UserConnection(string userName);
        Task UserConnections(List<CardGameActiveUserDto> activeUsers);
        Task RequestGameConnection(string userName);
        Task AcceptGameConnection();
        Task DeclineGameConnection();
    }
}