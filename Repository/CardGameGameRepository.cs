using Microsoft.EntityFrameworkCore;
using web_bite_server.Data;
using web_bite_server.Dtos.CardGame;
using web_bite_server.Models;

namespace web_bite_server.Repository
{
    public class CardGameGameRepository
    {
        private readonly ApplicationDBContext _dbContext;
        public CardGameGameRepository(ApplicationDBContext dBContext)
        {
            _dbContext = dBContext;
        }

        public async Task AddCardsToCardGameHand(CardGameConnection userConnection, IEnumerable<CardGameCard> cardGameCards)
        {
            userConnection.CardGameHand.AddRange(cardGameCards);
            userConnection.Round = 1;
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<CardGameCardDto>?> GetUserCardGameHand(CardGameConnection userConnection)
        {
            var cardGameConnection = await _dbContext.CardGameConnection.Include(i => i.CardGameHand).FirstOrDefaultAsync(c => c.Id == userConnection.Id);
            return cardGameConnection?.CardGameHand.Select(c => new CardGameCardDto
            {
                CardName = c.CardName,
                AttackValue = c.AttackValue,
                DefenseValue = c.DefenseValue,
                Id = c.Id,
                Label = c.Label,
                SpecialAbility = c.SpecialAbility
            }).ToList();
        }
    }
}