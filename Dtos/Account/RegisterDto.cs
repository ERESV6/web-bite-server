using System.ComponentModel.DataAnnotations;

namespace web_bite_server.Dtos.Account
{
    public class RegisterDto
    {
        [Required]
        public required string Username { get; set; }
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
        [Required]
        public required string Password { get; set; }
    }
}