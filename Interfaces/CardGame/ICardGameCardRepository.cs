using web_bite_server.Dtos.CardGame;

namespace web_bite_server.Interfaces.CardGame
{
    public interface ICardGameCardRepository
    {
        Task<List<CardGameCardDto>> GetAllCards();
    }
}