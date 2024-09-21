using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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

        // Dodaj karty do ręki gracza
        public async Task AddCardsToCardGameHand(CardGameConnection userConnection, IEnumerable<CardGameCard> cardGameCards)
        {
            userConnection.CardGamePlayerHand.AddRange(cardGameCards);
            userConnection.Round = 1;
            await _dbContext.SaveChangesAsync();
        }

        // Aktualizuj parametry gracza po zakończeniu tury
        public async Task UpdatePlayerAfterRoundEnds(CardGameConnection userConnection, int playerHitpoints, int round)
        {
            userConnection.HitPoints = playerHitpoints;
            userConnection.Round = round;

            await _dbContext.SaveChangesAsync();
        }

        // Pobierz zagrane karty użytkownika
        public async Task<List<CardGameCardDto>> GetUserCardGamePlayedCards(CardGameConnection userConnection)
        {
            var userCardGamePlayed = await _dbContext.CardGamePlayerPlayed
                .Where(p => p.CardGameConnectionId == userConnection.Id)
                .Select(i => i.CardGameCard)
                .Select(c => new CardGameCardDto
                {
                    CardName = c.CardName,
                    AttackValue = c.AttackValue,
                    DefenseValue = c.DefenseValue,
                    Id = c.Id,
                    Label = c.Label,
                    SpecialAbility = c.SpecialAbility
                })
                .ToListAsync();

            return userCardGamePlayed;
        }

        // Pobierz karty w ręce uzytkownika
        public async Task<List<CardGameCardDto>> GetUserCardGameHand(CardGameConnection userConnection)
        {
            var userCardGameHand = await _dbContext.CardGamePlayerHand
                .Where(p => p.CardGameConnectionId == userConnection.Id)
                .Select(i => i.CardGameCard)
                .Select(c => new CardGameCardDto
                {
                    CardName = c.CardName,
                    AttackValue = c.AttackValue,
                    DefenseValue = c.DefenseValue,
                    Id = c.Id,
                    Label = c.Label,
                    SpecialAbility = c.SpecialAbility
                })
                .ToListAsync();

            return userCardGameHand;
        }

        // Pobierz karty w ręce uzytkownika poza kartami zagranymi
        public async Task<List<CardGameCardDto>> GetUserCardGameHandExceptPlayed(CardGameConnection userConnection)
        {
            var userCardGameHand = await _dbContext.CardGamePlayerHand
                .Where(p => p.CardGameConnectionId == userConnection.Id && !p.WasPlayed)
                .Select(i => i.CardGameCard)
                .Select(c => new CardGameCardDto
                {
                    CardName = c.CardName,
                    AttackValue = c.AttackValue,
                    DefenseValue = c.DefenseValue,
                    Id = c.Id,
                    Label = c.Label,
                    SpecialAbility = c.SpecialAbility
                })
                .ToListAsync();

            return userCardGameHand;
        }

        // Pobierz karty w ręce użytkownika po ID, potrzebne do weryfikacji czy zagrane karty rzeczywiście istnieją
        public async Task<List<CardGameCardDto>?> GetUserCardGameHandByCardIds(CardGameConnection userConnection, List<int> cardGameIds)
        {
            var userCardGameHand = await _dbContext.CardGamePlayerHand
                .Where(p => p.CardGameConnectionId == userConnection.Id && cardGameIds.Contains(p.CardGameCardId))
                .Select(i => i.CardGameCard)
                .Select(c => new CardGameCardDto
                {
                    CardName = c.CardName,
                    AttackValue = c.AttackValue,
                    DefenseValue = c.DefenseValue,
                    Id = c.Id,
                    Label = c.Label,
                    SpecialAbility = c.SpecialAbility
                })
                .ToListAsync();

            return userCardGameHand;
        }

        // Dodaj zagrane karty użytkownika 
        public async Task AddPlayedCards(CardGameConnection userConnection, IEnumerable<CardGameCard> playedCards)
        {
            userConnection.CardGamePlayerPlayed.AddRange(playedCards);
            await _dbContext.SaveChangesAsync();
        }

        // Oznacz karty z ręki jako zagrane
        public async Task MarkPlayedCardsAsPlayedFromHand(CardGameConnection userConnection, IEnumerable<CardGameCard> playedCards)
        {
            var cardsIds = playedCards.Select(c => c.Id);
            var userCardGameHand = await _dbContext.CardGamePlayerHand
               .Where(p => p.CardGameConnectionId == userConnection.Id && cardsIds.Contains(p.CardGameCardId))
               .ToListAsync();
            userCardGameHand.ForEach(i => { i.WasPlayed = true; });
            await _dbContext.SaveChangesAsync();
        }

        // Usuń zagrane karty użytkownika
        public async Task DeletePlayedCards(CardGameConnection userConnection, IEnumerable<CardGameCard> playedCards)
        {
            var cardsIds = playedCards.Select(c => c.Id);
            var userCardGamePlayed = await _dbContext.CardGamePlayerPlayed
               .Where(p => p.CardGameConnectionId == userConnection.Id && cardsIds.Contains(p.CardGameCardId))
               .ToListAsync();

            _dbContext.CardGamePlayerPlayed.RemoveRange(userCardGamePlayed);
            await _dbContext.SaveChangesAsync();
        }

        public DbTransaction CardGameGameRepositoryTransaction()
        {
            var transaction = _dbContext.Database.BeginTransaction();
            return transaction.GetDbTransaction();
        }

    }

}