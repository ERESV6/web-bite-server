using System.ComponentModel.DataAnnotations;

namespace web_bite_server.Models
{
    public class GameConnection
    {
        public int Id {get; set;}
        public required string ConnectionId { get; set; }
        public string? UserToId {get; set;}
        public string? AppUserName { get; set; }
        public string? AppUserId { get; set; }
        public AppUser? AppUser {get; set;}
    }
}