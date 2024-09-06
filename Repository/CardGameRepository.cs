
using Microsoft.EntityFrameworkCore;
using web_bite_server.Data;
using web_bite_server.Dtos.CardGame;
using web_bite_server.Interfaces.CardGame;
using web_bite_server.Models;

namespace web_bite_server.Repository
{
    public class CardGameRepository : ICardGameRepository
    {
        private readonly ApplicationDBContext _dbContext;
        public CardGameRepository(ApplicationDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task CleanConnectionUsersPendingIds(GameConnection? userConnection, GameConnection? userToConnection)
        {
            if (userConnection == null || userToConnection == null)
            {
                return;
            }
            userConnection.UserToRequestPendingId = string.Empty;
            userToConnection.UserToRequestPendingId = string.Empty;
            await _dbContext.SaveChangesAsync();
        }

        public async Task ConnectConnectionUsersIds(GameConnection? userConnection, GameConnection? userToConnection)
        {
            if (userConnection == null || userToConnection == null)
            {
                return;
            }
            userConnection.UserToId = userConnection.UserToRequestPendingId;
            userToConnection.UserToId = userToConnection.UserToRequestPendingId;

            userConnection.UserToRequestPendingId = string.Empty;
            userToConnection.UserToRequestPendingId = string.Empty;

            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateConnectionUsersPendingIds(GameConnection? userConnection, GameConnection? userToConnection)
        {
            if (userConnection == null || userToConnection == null)
            {
                return;
            }
            userConnection.UserToRequestPendingId = userToConnection.AppUserId;
            userToConnection.UserToRequestPendingId = userConnection.AppUserId;
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<CardGameActiveUserDto>> GetAllActiveGameConnectionsAsync()
        {
            var gameConnections = await _dbContext.GameConnection.Include(i => i.AppUser).ToListAsync();
            // to mappers
            var activeGameConnections = gameConnections.Select(c => new CardGameActiveUserDto
            {
                ConnectionId = c.ConnectionId,
                UserName = c.AppUser.UserName,
                IsAvaliable = !(c.UserToId?.Length > 0 || c.UserToRequestPendingId?.Length > 0)
            }).ToList();

            return activeGameConnections;
        }

        public async Task<GameConnection?> GetGameConnectionByConnectionId(string? connectionId)
        {
            return await _dbContext.GameConnection.FirstOrDefaultAsync(gc => gc.ConnectionId == connectionId);
        }

        public async Task<GameConnection?> GetGameConnectionByGameConnectionId(int? gameConnectionId)
        {
            return await _dbContext.GameConnection.FirstOrDefaultAsync(gc => gc.Id == gameConnectionId);
        }

        public async Task RemoveGameConnection(GameConnection connection)
        {
            _dbContext.GameConnection.Remove(connection);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateUserGameConnectionOnReconnect(GameConnection connection, string connectionId)
        {
            connection.ConnectionId = connectionId;
            connection.UserToId = string.Empty;
            connection.UserToRequestPendingId = string.Empty;
            await _dbContext.SaveChangesAsync();
        }
    }
}