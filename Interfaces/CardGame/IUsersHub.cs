using SignalRSwaggerGen.Attributes;
using web_bite_server.Dtos.CardGame;

namespace web_bite_server.interfaces.CardGame
{
   
    [SignalRHub("")]
    public interface IUsersHub
    {
        Task UserConnection(string userName, List<CardGameActiveUserDto> activeUsers, string? connectionId);
        Task RequestGameConnection(string userConnectionId);
        Task CancelGameConnection(string userConnectionId);
        Task AcceptGameConnection(string userConnectionId);
        Task DeclineGameConnection(string userConnectionId);
    }
}