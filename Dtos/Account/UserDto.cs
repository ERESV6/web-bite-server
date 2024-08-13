namespace web_bite_server.Dtos.Account
{
    public class UserDto
    {
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required  string Token { get; set; }
    }
}