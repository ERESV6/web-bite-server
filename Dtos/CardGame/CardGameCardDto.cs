using System.ComponentModel.DataAnnotations;
using web_bite_server.Enums;

namespace web_bite_server.Dtos.CardGame
{
    public class CardGameCardDto
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public required string CardName { get; set; }
        public int AttackValue { get; set; }
        public int DefenseValue { get; set; }
        public string Label { get; set; } = string.Empty;
        public GameCardAbility SpecialAbility { get; set; }

    }
}