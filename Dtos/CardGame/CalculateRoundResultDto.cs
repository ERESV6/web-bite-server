

using System.ComponentModel.DataAnnotations;

namespace web_bite_server.Dtos.CardGame
{
    public class RoundResultDto
    {
        [Required]
        public int PlayerHitpoints { get; set; }
        [Required]
        public int EnemyHitpoints { get; set; }
        [Required]
        public int Round { get; set; }
        [Required]
        public int PlayerPoints { get; set; }
        [Required]
        public int EnemyPoints { get; set; }
        [Required]
        public int PlayerAttack { get; set; }
        [Required]
        public int PlayerDefense { get; set; }
        [Required]
        public int EnemyAttack { get; set; }
        [Required]
        public int EnemyDefense { get; set; }
        [Required]
        public int DamageDoneToEnemy { get; set; }
        [Required]
        public int DamageDoneToPlayer { get; set; }
        [Required]
        public List<CardGameCardDto> PlayerPlayedCards { get; set; } = [];
        [Required]
        public List<CardGameCardDto> EnemyPlayedCards { get; set; } = [];
        public bool IsEndRound { get; set; } = false;
        public bool IsPlayerWinner { get; set; } = false;
        public bool IsEnemyWinner { get; set; } = false;
    }
}