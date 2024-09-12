
namespace web_bite_server.Models
{
    public class CardGameConnection
    {
        public int Id { get; set; }
        public required string ConnectionId { get; set; }
        public string UserToId { get; set; } = string.Empty;
        public string UserToRequestPendingId { get; set; } = string.Empty;
        public required string AppUserId { get; set; }
        public int Round { get; set; } = 0;
        public AppUser? AppUser { get; set; }
        public List<CardGameCard> CardGameHand { get; set; } = [];
        public List<CardGameCard> CardGamePlayed { get; set; } = [];
    }
}