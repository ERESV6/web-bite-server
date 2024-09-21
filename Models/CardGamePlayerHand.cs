namespace web_bite_server.Models
{
    public class CardGamePlayerHand
    {
        public int Id { get; set; }
        public bool WasPlayed { get; set; } = false;
        public int CardGameCardId { get; set; }
        public int CardGameConnectionId { get; set; }
        public CardGameCard CardGameCard { get; set; } = null!;
        public CardGameConnection CardGameConnection { get; set; } = null!;
    }
}