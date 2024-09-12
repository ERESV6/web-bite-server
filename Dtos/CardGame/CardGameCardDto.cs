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
        [Required]
        public int AttackValue { get; set; }
        [Required]
        public int DefenseValue { get; set; }
        [Required]
        public string Label { get; set; } = string.Empty;
        [Required]
        public CardGameCardAbility SpecialAbility { get; set; }

    }
}