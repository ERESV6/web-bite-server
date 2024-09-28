using Microsoft.AspNetCore.SignalR;
using web_bite_server.Constants;
using web_bite_server.Dtos.CardGame;
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
        public async Task<List<CardGameCardDto>?> AddCardsToCardGameHand(List<int> cardGameIds, CardGameUsersConnectionDto cardGameUsersConnectionDto)
        {
            if (!cardGameUsersConnectionDto.UserConnection.IsEndTurn)
            {
                throw new BadHttpRequestException("USER TURN IS IN PROGRESS");
            }
            if (cardGameIds.Count < CardGameConfig.MinCardsToAdd || cardGameIds.Count > CardGameConfig.MaxCardsToAdd)
            {
                throw new BadHttpRequestException("NUMBER OF CARD IDS MUST BE BETWEEN" + CardGameConfig.MinCardsToAdd + "TO" + CardGameConfig.MaxCardsToAdd);
            }

            var cardGameCards = await _cardGameCardRepository.GetCardsByIds(cardGameIds);
            if (cardGameCards.Count != cardGameIds.Count)
            {
                throw new BadHttpRequestException("SOME CARD IDS DOESN'T EXISTS");
            }

            var cardGameHand = await _cardGameGameRepository.GetUserCardGameHandExceptPlayed(cardGameUsersConnectionDto.UserConnection);
            if (cardGameHand.Count + cardGameCards.Count > CardGameConfig.MaxCardsInHand)
            {
                throw new BadHttpRequestException("SUM OF CARDS IS MORE THAN " + CardGameConfig.MaxCardsInHand);
            }
            var round = cardGameUsersConnectionDto.UserConnection.Round + 1;
            await _cardGameGameRepository.AddCardsToCardGameHand(cardGameUsersConnectionDto.UserConnection, cardGameCards, round);
            await _hubContext.Clients.Client(cardGameUsersConnectionDto.EnemyUserConnection.ConnectionId).SendRound(round);

            return [.. cardGameHand, .. cardGameCards];
        }

        // Koniec tury
        public async Task EndTurn(List<CardGameCardDto> playedCards, CardGameUsersConnectionDto cardGameUsersConnectionDto)
        {
            if (playedCards.Count == 0)
            {
                throw new BadHttpRequestException("YOU HAVE TO PLAY AT LEAST ONE CARD");
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
            if (!cardGameUsersConnectionDto.UserConnection.IsEndTurn)
            {
                throw new BadHttpRequestException("CANT CALCULATE ROUND RESULT WHEN TURN IS IN PROGRESS");
            }
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
            var round = cardGameUsersConnectionDto.UserConnection.Round;
            var roundPoints = cardGameUsersConnectionDto.UserConnection.RoundPoints;

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

                if (cardGameUsersConnectionDto.UserConnection.Round >= CardGameConfig.MaxRoundsToEndGame || enemyHitpoints <= 0 || playerHitpoints <= 0)
                {
                    await _cardGameGameRepository.DeletePlayerHandCards(cardGameUsersConnectionDto.UserConnection);
                    if (playerHitpoints > enemyHitpoints || enemyHitpoints <= 0)
                    {
                        roundPoints++;
                        await _cardGameGameRepository.AddRoundPoints(cardGameUsersConnectionDto.UserConnection, roundPoints);
                    }
                    round = 0;
                    playerHitpoints = CardGameConfig.UserHitPoints;
                    enemyHitpoints = CardGameConfig.UserHitPoints;
                    await _cardGameGameRepository.ResetPlayerParams(cardGameUsersConnectionDto.UserConnection);
                }

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
                Round = round,
                Points = roundPoints
            };
        }

        // Sprawdza czy zagrane karty rzeczywiście istnieją
        public async Task<List<CardGameCardDto>> CheckPlayedCards(List<int> cardGameIds, CardGameUsersConnectionDto cardGameUsersConnectionDto)
        {
            if (cardGameUsersConnectionDto.UserConnection.IsEndTurn)
            {
                throw new BadHttpRequestException("CANT PLAY CARD WHEN TURN IS ENDED");
            }
            if (cardGameUsersConnectionDto.UserConnection.Round != cardGameUsersConnectionDto.EnemyUserConnection.Round)
            {
                throw new BadHttpRequestException("PLAYERS ROUND DONT MATCH");
            }
            var cardGameHand = await _cardGameGameRepository.GetUserCardGameHandByCardIds(cardGameUsersConnectionDto.UserConnection, cardGameIds);
            if (cardGameIds.Count != cardGameHand?.Count)
            {
                throw new BadHttpRequestException("SOME CARD IDS DOESN'T EXISTS");
            }
            return cardGameHand;
        }
    }
}

