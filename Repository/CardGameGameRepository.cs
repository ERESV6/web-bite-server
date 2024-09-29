using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using web_bite_server.Constants;
using web_bite_server.Data;
using web_bite_server.Dtos.CardGame;
using web_bite_server.Mappers;
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

        // Dodaj karty do ręki gracza - dodaj karty, zwiększ runde, ustaw flagę końca tury na false
        public async Task AddCardsToCardGameHand(CardGameConnection userConnection, List<int> cardGameIds, int round)
        {
            var cardGameCards = cardGameIds.Select(i => new CardGamePlayerHand
            {
                CardGameCardId = i,
                CardGameConnectionId = userConnection.Id,
                WasPlayed = false
            });

            userConnection.Round = round;
            userConnection.IsEndTurn = false;
            _dbContext.CardGamePlayerHand.AddRange(cardGameCards);

            await _dbContext.SaveChangesAsync();
        }

        // Aktualizuj punkty zdrowia gracza po zakończeniu tury
        public async Task UpdatePlayerHPAfterRoundEnds(CardGameConnection userConnection, int playerHitpoints)
        {
            userConnection.HitPoints = playerHitpoints;
            await _dbContext.SaveChangesAsync();
        }


        // Aktualizuj parametry gracza po zakończeniu rundy
        public async Task ResetPlayerParams(CardGameConnection userConnection)
        {
            userConnection.Round = 0;
            userConnection.HitPoints = CardGameConfig.UserHitPoints;
            await _dbContext.SaveChangesAsync();
        }

        // Dodaj punkt za wygraną rundę
        public async Task AddRoundPoints(CardGameConnection userConnection, int roundPoints)
        {
            userConnection.RoundPoints = roundPoints;
            await _dbContext.SaveChangesAsync();
        }

        // Pobierz zagrane karty użytkownika
        public async Task<List<CardGameCardDto>> GetUserCardGamePlayedCards(CardGameConnection userConnection)
        {
            var userCardGamePlayed = await _dbContext.CardGamePlayerPlayed
                .Where(p => p.CardGameConnectionId == userConnection.Id)
                .Select(i => i.CardGameCard)
                .Select(c => c.ToCardGameCardDto())
                .ToListAsync();

            return userCardGamePlayed;
        }

        // Pobierz karty w ręce uzytkownika
        public async Task<List<CardGameCardDto>> GetUserCardGameHand(CardGameConnection userConnection)
        {
            var userCardGameHand = await _dbContext.CardGamePlayerHand
                .Where(p => p.CardGameConnectionId == userConnection.Id)
                .Select(i => i.CardGameCard)
                .Select(c => c.ToCardGameCardDto())
                .ToListAsync();

            return userCardGameHand;
        }

        // Pobierz karty w ręce uzytkownika
        public async Task<List<CardGameCardDto>> GetUserCardGameHandExceptPlayed(CardGameConnection userConnection)
        {
            var userCardGameHand = await _dbContext.CardGamePlayerHand
                .Where(p => p.CardGameConnectionId == userConnection.Id && !p.WasPlayed)
                .Select(i => i.CardGameCard)
                .Select(c => c.ToCardGameCardDto())
                .ToListAsync();

            return userCardGameHand;
        }

        // Pobierz karty w ręce użytkownika po ID, potrzebne do weryfikacji czy zagrane karty rzeczywiście istnieją
        public async Task<List<CardGameCardDto>?> GetUserCardGameHandByCardIds(CardGameConnection userConnection, List<int> cardGameIds)
        {
            var userCardGameHand = await _dbContext.CardGamePlayerHand
                .Where(p => p.CardGameConnectionId == userConnection.Id && cardGameIds.Contains(p.CardGameCardId) && !p.WasPlayed)
                .Select(i => i.CardGameCard)
                .Select(c => c.ToCardGameCardDto())
                .ToListAsync();

            return userCardGameHand;
        }

        // Dodaj zagrane karty użytkownika, ustaw flagę zakończenia tury na true
        public async Task AddPlayedCards(CardGameConnection userConnection, IEnumerable<int> playedCardIds)
        {
            var cardGamePlayedCards = playedCardIds.Select(i => new CardGamePlayerPlayed
            {
                CardGameCardId = i,
                CardGameConnectionId = userConnection.Id
            });

            _dbContext.CardGamePlayerPlayed.AddRange(cardGamePlayedCards);
            userConnection.IsEndTurn = true;
            await _dbContext.SaveChangesAsync();
        }

        // Oznacz karty z ręki jako zagrane
        public async Task MarkPlayedCardsAsPlayedFromHand(CardGameConnection userConnection, IEnumerable<int> playedCardIds)
        {
            var userCardGameHand = await _dbContext.CardGamePlayerHand
               .Where(p => p.CardGameConnectionId == userConnection.Id && playedCardIds.Contains(p.CardGameCardId))
               .ToListAsync();
            userCardGameHand.ForEach(i => { i.WasPlayed = true; });
            await _dbContext.SaveChangesAsync();
        }

        // Usuń zagrane karty użytkownika
        public async Task DeletePlayedCards(CardGameConnection userConnection, IEnumerable<int> playedCardIds)
        {
            var userCardGamePlayed = await _dbContext.CardGamePlayerPlayed
               .Where(p => p.CardGameConnectionId == userConnection.Id && playedCardIds.Contains(p.CardGameCardId))
               .ToListAsync();

            _dbContext.CardGamePlayerPlayed.RemoveRange(userCardGamePlayed);
            await _dbContext.SaveChangesAsync();
        }

        // Usuń karty użytkownika z ręki
        public async Task DeletePlayerHandCards(CardGameConnection userConnection)
        {
            var userCardGameHand = await _dbContext.CardGamePlayerHand
               .Where(p => p.CardGameConnectionId == userConnection.Id)
               .ToListAsync();

            _dbContext.CardGamePlayerHand.RemoveRange(userCardGameHand);
            await _dbContext.SaveChangesAsync();
        }

        public DbTransaction CardGameGameRepositoryTransaction()
        {
            var transaction = _dbContext.Database.BeginTransaction();
            return transaction.GetDbTransaction();
        }

    }

}