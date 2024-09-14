namespace web_bite_server.Models
{
    public class CardGamePlayerPlayed
    {
        public int Id { get; set; }
        public int CardGameCardId { get; set; }
        public int CardGameConnectionId { get; set; }
        public CardGameCard CardGameCard { get; set; } = null!;
        public CardGameConnection CardGameConnection { get; set; } = null!;
    }
}