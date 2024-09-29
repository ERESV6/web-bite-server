
using Microsoft.EntityFrameworkCore;
using web_bite_server.Constants;
using web_bite_server.Data;
using web_bite_server.Dtos.CardGame;
using web_bite_server.Mappers;
using web_bite_server.Models;

namespace web_bite_server.Repository
{
    public class CardGameConnectionRepository
    {
        private readonly ApplicationDBContext _dbContext;
        public CardGameConnectionRepository(ApplicationDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task CleanConnectionUsersPendingIds(CardGameConnection? userConnection, CardGameConnection? userToConnection)
        {
            if (userConnection == null || userToConnection == null)
            {
                return;
            }
            userConnection.UserToRequestPendingId = string.Empty;
            userToConnection.UserToRequestPendingId = string.Empty;
            await _dbContext.SaveChangesAsync();
        }

        public async Task ConnectConnectionUsersIds(CardGameConnection? userConnection, CardGameConnection? userToConnection)
        {
            if (userConnection == null || userToConnection == null)
            {
                return;
            }
            userConnection.UserToId = userConnection.UserToRequestPendingId;
            userToConnection.UserToId = userToConnection.UserToRequestPendingId;
            userConnection.HitPoints = CardGameConfig.UserHitPoints;
            userToConnection.HitPoints = CardGameConfig.UserHitPoints;

            userConnection.UserToRequestPendingId = string.Empty;
            userToConnection.UserToRequestPendingId = string.Empty;

            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateConnectionUsersPendingIds(CardGameConnection? userConnection, CardGameConnection? userToConnection)
        {
            if (userConnection == null || userToConnection == null)
            {
                return;
            }
            userConnection.UserToRequestPendingId = userToConnection.AppUserId;
            userToConnection.UserToRequestPendingId = userConnection.AppUserId;
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<CardGameActiveUserDto>> GetAllActiveCardGameConnectionsAsync()
        {
            var CardGameConnections = await _dbContext.CardGameConnection.Include(i => i.AppUser).ToListAsync();
            var activeCardGameConnections = CardGameConnections.Select(c => c.ToCardGameActiveUserDto()).ToList();
            return activeCardGameConnections;
        }

        public async Task<CardGameConnection?> GetCardGameConnectionByConnectionId(string? connectionId)
        {
            return await _dbContext.CardGameConnection.FirstOrDefaultAsync(gc => gc.ConnectionId == connectionId);
        }

        public async Task<CardGameConnection?> GetCardGameConnectionByCardGameConnectionId(int? CardGameConnectionId)
        {
            return await _dbContext.CardGameConnection.Include(i => i.AppUser).FirstOrDefaultAsync(gc => gc.Id == CardGameConnectionId);
        }

        public async Task RemoveCardGameConnection(CardGameConnection connection)
        {
            _dbContext.CardGameConnection.Remove(connection);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateUserCardGameConnectionOnReconnect(CardGameConnection connection, string connectionId)
        {
            connection.ConnectionId = connectionId;
            connection.UserToId = string.Empty;
            connection.UserToRequestPendingId = string.Empty;
            await _dbContext.SaveChangesAsync();
        }
    }
}