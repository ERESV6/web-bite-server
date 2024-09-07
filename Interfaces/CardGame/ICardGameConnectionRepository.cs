
using web_bite_server.Dtos.CardGame;
using web_bite_server.Models;

namespace web_bite_server.Interfaces.CardGame
{
    public interface ICardGameConnectionRepository
    {
        Task<List<CardGameActiveUserDto>> GetAllActiveCardGameConnectionsAsync();
        Task<CardGameConnection?> GetCardGameConnectionByCardGameConnectionId(int? CardGameConnectionId);
        Task<CardGameConnection?> GetCardGameConnectionByConnectionId(string? connectionId);
        Task UpdateConnectionUsersPendingIds(CardGameConnection? userConnection, CardGameConnection? userToConnection);
        Task CleanConnectionUsersPendingIds(CardGameConnection? userConnection, CardGameConnection? userToConnection);
        Task ConnectConnectionUsersIds(CardGameConnection? userConnection, CardGameConnection? userToConnection);
        Task RemoveCardGameConnection(CardGameConnection connection);
        Task UpdateUserCardGameConnectionOnReconnect(CardGameConnection connection, string connectionId);
    }
}