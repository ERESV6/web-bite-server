
using web_bite_server.Models;

namespace web_bite_server.Dtos.CardGame
{
    public class CardGameUsersConnectionDto
    {
        public required CardGameConnection UserConnection { get; set; }
        public required CardGameConnection EnemyUserConnection { get; set; }
    }
}