
using web_bite_server.Dtos.CardGame;
using web_bite_server.Models;

namespace web_bite_server.Interfaces.CardGame
{
    public interface ICardGameRepository
    {
        Task<List<CardGameActiveUserDto>> GetAllActiveGameConnectionsAsync();
        Task<GameConnection?> GetGameConnectionByGameConnectionId(int? gameConnectionId);
        Task<GameConnection?> GetGameConnectionByConnectionId(string? connectionId);
        Task UpdateConnectionUsersPendingIds(GameConnection? userConnection, GameConnection? userToConnection);
        Task CleanConnectionUsersPendingIds(GameConnection? userConnection, GameConnection? userToConnection);
        Task ConnectConnectionUsersIds(GameConnection? userConnection, GameConnection? userToConnection);
        Task RemoveGameConnection(GameConnection connection);
        Task UpdateUserGameConnectionOnReconnect(GameConnection connection, string connectionId);
    }
}