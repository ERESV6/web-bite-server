using Microsoft.EntityFrameworkCore;
using web_bite_server.Data;
using web_bite_server.Dtos.CardGame;

namespace web_bite_server.Repository
{
    public class CardGameCardRepository
    {
        private readonly ApplicationDBContext _dBContext;
        public CardGameCardRepository(ApplicationDBContext dBContext)
        {
            _dBContext = dBContext;
        }
        public async Task<List<CardGameCardDto>> GetAllCards()
        {
            // todo mappers
            return await _dBContext.CardGameCard.Select(c => new CardGameCardDto
            {
                CardName = c.CardName,
                AttackValue = c.AttackValue,
                DefenseValue = c.DefenseValue,
                Id = c.Id,
                Label = c.Label,
                SpecialAbility = c.SpecialAbility
            }).ToListAsync();
        }
    }
}