using web_bite_server.Enums;

namespace web_bite_server.Models
{
    public class GameCard
    {
        public int Id { get; set; }
        public string CardName { get; set; } = string.Empty;
        public int AttackValue { get; set; }
        public int DefenseValue { get; set; }
        public string Label { get; set; } = string.Empty;
        public GameCardAbility SpecialAbility { get; set; }
    }
}