using web_bite_server.Enums;

namespace web_bite_server.Models
{
    public class CardGameCard
    {
        public int Id { get; set; }
        public string CardName { get; set; } = string.Empty;
        public int AttackValue { get; set; }
        public int DefenseValue { get; set; }
        public string Label { get; set; } = string.Empty;
        public CardGameCardAbility SpecialAbility { get; set; }
        public List<CardGameConnection> CardGamePlayerHand { get; } = [];
    }
}