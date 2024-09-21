using Microsoft.EntityFrameworkCore;
using web_bite_server.Data;
using web_bite_server.Dtos.CardGame;
using web_bite_server.Models;

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

        public async Task<List<CardGameCard>> GetCardsByIds(List<int> cardGameIds)
        {
            // todo mappers
            return await _dBContext.CardGameCard.Where(c => cardGameIds.Contains(c.Id)).ToListAsync();
        }

        public async Task<List<CardGameCardDto>> GetCardsExceptIds(IEnumerable<int> cardGameIds)
        {
            // todo mappers
            return await _dBContext.CardGameCard.Where(c => !cardGameIds.Contains(c.Id)).Select(c => new CardGameCardDto
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