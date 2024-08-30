using System.ComponentModel.DataAnnotations;

namespace web_bite_server.Dtos.CardGame
{
    public class CardGameConnectionDto
    {
        [Required]
        public required string UserConnectionId { get; set; }
        [Required]
        public required string UserToConnectionId { get; set; }
    }
}