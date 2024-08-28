using Microsoft.AspNetCore.Identity;

namespace web_bite_server.Models
{
    public class AppUser : IdentityUser
    {
        public int? GameConnectionId { get; set; }
        public GameConnection? GameConnection {get; set;}
    }
}