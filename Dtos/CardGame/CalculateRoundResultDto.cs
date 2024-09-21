

namespace web_bite_server.Dtos.CardGame
{
    public class RoundResultDto
    {
        public int PlayerHitpoints { get; set; }
        public int EnemyHitpoints { get; set; }
        public int Round { get; set; }
        public int Points { get; set; }
    }
}