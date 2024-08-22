using web_bite_server.Dtos.CardGame;

namespace web_bite_server.interfaces.CardGame
{
    public interface IUsersHub
    {
        Task GetAllActiveUsers(List<CardGameActiveUserDto> activeUsers);
        Task UserConnection(string message);
    }
}