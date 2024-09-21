using Microsoft.AspNetCore.SignalR;
using web_bite_server.Dtos.CardGame;
using web_bite_server.Exceptions;
using web_bite_server.Hubs;
using web_bite_server.interfaces.CardGame;
using web_bite_server.Models;
using web_bite_server.Repository;

namespace web_bite_server.Services.CardGame
{
    public class CardGameGameService
    {
        private readonly CardGameGameRepository _cardGameGameRepository;
        private readonly CardGameCardRepository _cardGameCardRepository;
        private readonly IHubContext<CardGameHub, ICardGameHub> _hubContext;
        public CardGameGameService(
            CardGameGameRepository cardGameGameRepository,
            CardGameCardRepository cardGameCardRepository,
            IHubContext<CardGameHub, ICardGameHub> hubContext

        )
        {
            _cardGameGameRepository = cardGameGameRepository;
            _cardGameCardRepository = cardGameCardRepository;
            _hubContext = hubContext;
        }

        // Pobiera wszystkie karty, poza wcześniej wybranymi
        public async Task<List<CardGameCardDto>> GetAllCardsExceptPlayerHand(CardGameConnection userConnection)
        {
            var cardGameHand = await _cardGameGameRepository.GetUserCardGameHand(userConnection);
            return await _cardGameCardRepository.GetCardsExceptIds(cardGameHand.Select(i => i.Id));
        }

        // Pobiera liczbę kart w ręce przeciwnika
        public async Task<int> GetNumberOfEnemyPlayerHand(CardGameConnection userConnection)
        {
            var cardGameHand = await _cardGameGameRepository.GetUserCardGameHandExceptPlayed(userConnection);
            return cardGameHand.Count;
        }

        // Dodaje wybrane karty do ręki użytkownika
        public async Task<List<CardGameCardDto>?> AddCardsToCardGameHand(List<int> cardGameIds, CardGameUsersConnectionDto? cardGameUsersConnectionDto)
        {
            if (cardGameUsersConnectionDto?.UserConnection == null)
            {
                throw new NotFoundException("CONNECTED USER NOT FOUND");
            }

            // @TODO const z backendu
            if (cardGameIds.Count <= 0 && cardGameIds.Count > 5)
            {
                throw new BadHttpRequestException("NUMBER OF CARD IDS MUST BE BETWEEN 1 TO 5");
            }

            var cardGameCards = await _cardGameCardRepository.GetCardsByIds(cardGameIds);
            // @TODO const z backendu
            if (cardGameCards.Count != cardGameIds.Count)
            {
                throw new BadHttpRequestException("SOME CARD IDS DOESN'T EXISTS");
            }

            var cardGameHand = await _cardGameGameRepository.GetUserCardGameHandExceptPlayed(cardGameUsersConnectionDto.UserConnection);
            if (cardGameHand.Count + cardGameCards.Count > 10)
            {
                throw new BadHttpRequestException("SUM OF CARDS IS MORE THAN 10");
            }

            var round = cardGameUsersConnectionDto.UserConnection.Round + 1;
            await _cardGameGameRepository.AddCardsToCardGameHand(cardGameUsersConnectionDto.UserConnection, cardGameCards, round);
            await _hubContext.Clients.Client(cardGameUsersConnectionDto.EnemyUserConnection.ConnectionId).SendRound(round);

            return [.. cardGameHand, .. cardGameCards];
        }

        // Sprawdza czy zagrane karty rzeczywiście istnieją
        public async Task<List<CardGameCardDto>> CheckPlayedCards(List<int> cardGameIds, CardGameUsersConnectionDto cardGameUsersConnectionDto)
        {
            var cardGameHand = await _cardGameGameRepository.GetUserCardGameHandByCardIds(cardGameUsersConnectionDto.UserConnection, cardGameIds);
            if (cardGameIds.Count != cardGameHand?.Count)
            {
                throw new BadHttpRequestException("SOME CARD IDS DOESN'T EXISTS");
            }

            if (cardGameUsersConnectionDto.UserConnection.Round != cardGameUsersConnectionDto.EnemyUserConnection.Round)
            {
                throw new BadHttpRequestException("PLAYERS ROUND DONT MATCH");
            }
            return cardGameHand;
        }

        // Koniec tury
        public async Task EndTurn(List<CardGameCardDto> playedCards, CardGameUsersConnectionDto cardGameUsersConnectionDto)
        {
            if (playedCards.Count == 0)
            {
                throw new BadHttpRequestException("YOU HAVE TO PLAY AT LEAST ONE CARD");
            }
            var cardGamePlayedCards = await _cardGameGameRepository.GetUserCardGamePlayedCards(cardGameUsersConnectionDto.UserConnection);
            if (cardGamePlayedCards.Count != 0)
            {
                throw new BadHttpRequestException("PLAYER ALREADY ENDED HIS TURN");
            }
            var cardGameCard = playedCards.Select(c => new CardGameCard
            {
                AttackValue = c.AttackValue,
                CardName = c.CardName,
                DefenseValue = c.DefenseValue,
                Id = c.Id,
                Label = c.Label,
                SpecialAbility = c.SpecialAbility
            });

            using var transaction = _cardGameGameRepository.CardGameGameRepositoryTransaction();
            try
            {
                await _cardGameGameRepository.AddPlayedCards(cardGameUsersConnectionDto.UserConnection, cardGameCard);
                await _cardGameGameRepository.MarkPlayedCardsAsPlayedFromHand(cardGameUsersConnectionDto.UserConnection, cardGameCard);

                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw new Exception("TRANSACTION ROLLED BACK");
            }
        }

        // Sprawdza zagrane karty obu graczy i zlicza odebrane punkty zdrowia
        public async Task<RoundResultDto> CalculateRoundResult(CardGameUsersConnectionDto cardGameUsersConnectionDto)
        {
            var userPlayedCards = await _cardGameGameRepository.GetUserCardGamePlayedCards(cardGameUsersConnectionDto.UserConnection);
            if (userPlayedCards.Count == 0)
            {
                throw new BadHttpRequestException("USER DID NOT PLAY ANY CARD");
            }
            var enemyPlayedCards = await _cardGameGameRepository.GetUserCardGamePlayedCards(cardGameUsersConnectionDto.EnemyUserConnection);
            if (enemyPlayedCards.Count == 0)
            {
                throw new BadHttpRequestException("ENEMY DID NOT PLAY ANY CARD");
            }

            var playerHitpoints = cardGameUsersConnectionDto.UserConnection.HitPoints;
            var enemyHitpoints = cardGameUsersConnectionDto.EnemyUserConnection.HitPoints;

            var playerAttack = 0;
            var playerDefense = 0;
            var enemyAttack = 0;
            var enemyDefense = 0;

            userPlayedCards.ForEach(card =>
            {
                playerAttack += card.AttackValue;
                playerDefense += card.DefenseValue;
            });

            enemyPlayedCards.ForEach(card =>
            {
                enemyAttack += card.AttackValue;
                enemyDefense += card.DefenseValue;
            });

            playerHitpoints += -(enemyAttack - playerDefense > 0 ? enemyAttack - playerDefense : 0);
            enemyHitpoints += -(playerAttack - enemyDefense > 0 ? playerAttack - enemyDefense : 0);

            var cardGameCard = userPlayedCards.Select(c => new CardGameCard
            {
                AttackValue = c.AttackValue,
                CardName = c.CardName,
                DefenseValue = c.DefenseValue,
                Id = c.Id,
                Label = c.Label,
                SpecialAbility = c.SpecialAbility
            });

            using var transaction = _cardGameGameRepository.CardGameGameRepositoryTransaction();
            try
            {
                await _cardGameGameRepository.UpdatePlayerHPAfterRoundEnds(cardGameUsersConnectionDto.UserConnection, playerHitpoints);
                await _cardGameGameRepository.DeletePlayedCards(cardGameUsersConnectionDto.UserConnection, cardGameCard);

                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw new Exception("TRANSACTION ROLLED BACK");
            }

            return new RoundResultDto
            {
                PlayerHitpoints = playerHitpoints,
                EnemyHitpoints = enemyHitpoints,
            };
        }
    }
}

