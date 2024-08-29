using System.ComponentModel.DataAnnotations;

namespace web_bite_server.Dtos.CardGame
{
    public class CardGameActiveUserDto
    {
        [Required]
        public required string ConnectionId { get; set; }
        [Required]
        public required string UserName { get; set; }
        public bool IsAvaliable { get; set; } = true;
    }
}