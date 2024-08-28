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
                        }                        
                    } 

                    await _userManager.UpdateAsync(appUser);            
                    await _dBContext.SaveChangesAsync();    

                    var updatedConnections = await _dBContext.GameConnection.ToListAsync();
                    var asd = updatedConnections.Select(c => new CardGameActiveUserDto {
                        ConnectionId = c.ConnectionId,
                        UserName = c.AppUserName
                    }).ToList();
                    await Clients.All.UserConnection($"{userName} has joined", asd, connectionId);            
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
                    await _userManager.UpdateAsync(appUser);  
                    _dBContext.GameConnection.Remove(connection);
                    await _dBContext.SaveChangesAsync();    
                    var updatedConnections = await _dBContext.GameConnection.ToListAsync();
                    var asd = updatedConnections.Select(c => new CardGameActiveUserDto {
                        ConnectionId = c.ConnectionId,
                        UserName = c.AppUserName
                    }).ToList();
                    await Clients.All.UserConnection($"{userName} leave.", asd, null);                                       
                }
            }    
            
            await base.OnDisconnectedAsync(exception);          
        }   
    }
}