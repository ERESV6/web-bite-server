using Microsoft.AspNetCore.Identity;

namespace web_bite_server.Models
{
    public class AppUser : IdentityUser
    {
        public int? CardGameConnectionId { get; set; }
        public CardGameConnection? CardGameConnection { get; set; }
    }
}