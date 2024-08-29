using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using web_bite_server.Data;
using web_bite_server.Dtos.CardGame;
using web_bite_server.interfaces.CardGame;
using web_bite_server.Models;

namespace web_bite_server.Hubs
{
    [Authorize]
    public class UsersHub(UserManager<AppUser> userManager, ApplicationDBContext dBContext) : Hub<IUsersHub>
    {
        UserManager<AppUser> _userManager = userManager;
        ApplicationDBContext _dBContext = dBContext;

        /**
            - przeanalizować logikę, ogarnąć czy można zmniejszyć ilość strzałów na bazę            
            - dodać repository
            - sprawdzić czy można coś wyciągnąc stąd i api requestami ogarnać (raczej nie)
         */

        public override async Task OnConnectedAsync()
        {          
            var connectionId = Context.ConnectionId;
            var userName = Context.User?.Identity?.Name;
            if (userName != null)
            {
                
                var appUser = await _userManager.FindByNameAsync(userName);            
                if (appUser != null)
                {
                    if (appUser.GameConnectionId == null)
                    {
                        appUser.GameConnection = new GameConnection {
                            ConnectionId = connectionId,
                            AppUserId = appUser.Id,
                            AppUserName = appUser.UserName
                        };                
                    } else
                    {
                        var gameConnection = await _dBContext.GameConnection.FindAsync(appUser.GameConnectionId);
                        if (gameConnection != null)
                        {
                            gameConnection.ConnectionId = connectionId;
                            gameConnection.UserToId = string.Empty;
                            gameConnection.UserToRequestPendingId = string.Empty;
                        }                        
                    } 

                    await _userManager.UpdateAsync(appUser);            
                    await _dBContext.SaveChangesAsync();    
                    await SendConnectionMessage($"{userName} has joined.");         
                }
            }                                                           
            
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {           
            var userName = Context.User?.Identity?.Name;            
            var connection = await _dBContext.GameConnection.FirstOrDefaultAsync(m => m.ConnectionId == Context.ConnectionId);
            if (connection != null && userName != null)
            {
                var appUser = await _userManager.FindByNameAsync(userName); 
                if (appUser != null)
                {
                    appUser.GameConnectionId = null; 
                    _dBContext.GameConnection.Remove(connection);
                    await _userManager.UpdateAsync(appUser);  
                    await _dBContext.SaveChangesAsync();  
                    await SendConnectionMessage($"{userName} leave.");                                    
                }
            }    
            
            await base.OnDisconnectedAsync(exception);          
        }   

        private async Task SendConnectionMessage(string message)
        {
            var updatedConnections = await _dBContext.GameConnection.ToListAsync();
            var activeUsers = updatedConnections.Select(c => new CardGameActiveUserDto {
                ConnectionId = c.ConnectionId,
                UserName = c.AppUserName,
                IsAvaliable = !(c.UserToId?.Length > 0 || c.UserToRequestPendingId?.Length > 0)
            }).ToList();
            await Clients.All.UserConnection(message, activeUsers);
        }
    }
}