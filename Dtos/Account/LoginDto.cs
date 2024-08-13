using System.ComponentModel.DataAnnotations;

namespace web_bite_server.Dtos.Account
{
    public class LoginDto
    {
        [Required]
        public required string Email { get; set; }
        [Required]
        public required string Password { get; set; }
    }
}