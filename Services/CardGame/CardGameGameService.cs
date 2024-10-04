using Microsoft.AspNetCore.SignalR;
using web_bite_server.Constants;
using web_bite_server.Dtos.CardGame;
using web_bite_server.Enums;
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
            await _cardGameGameRepository.AddCardsToCardGameHand(cardGameUsersConnectionDto.UserConnection, cardGameIds, round);
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

            using var transaction = _cardGameGameRepository.CardGameGameRepositoryTransaction();
            try
            {
                var playedCardIds = playedCards.Select(c => c.Id);
                await _cardGameGameRepository.AddPlayedCards(cardGameUsersConnectionDto.UserConnection, playedCardIds);
                await _cardGameGameRepository.MarkPlayedCardsAsPlayedFromHand(cardGameUsersConnectionDto.UserConnection, playedCardIds);

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
            if (!cardGameUsersConnectionDto.UserConnection.IsEndTurn || !cardGameUsersConnectionDto.EnemyUserConnection.IsEndTurn)
            {
                throw new BadHttpRequestException("CANT CALCULATE ROUND RESULT WHEN TURN IS IN PROGRESS");
            }
            var playerPlayedCards = await _cardGameGameRepository.GetUserCardGamePlayedCards(cardGameUsersConnectionDto.UserConnection);
            if (playerPlayedCards.Count == 0)
            {
                throw new BadHttpRequestException("USER DID NOT PLAY ANY CARD");
            }
            var enemyPlayedCards = await _cardGameGameRepository.GetUserCardGamePlayedCards(cardGameUsersConnectionDto.EnemyUserConnection);
            if (enemyPlayedCards.Count == 0)
            {
                throw new BadHttpRequestException("ENEMY DID NOT PLAY ANY CARD");
            }

            var roundResults = new RoundResultDto
            {
                PlayerHitpoints = cardGameUsersConnectionDto.UserConnection.HitPoints,
                EnemyHitpoints = cardGameUsersConnectionDto.EnemyUserConnection.HitPoints,
                Round = cardGameUsersConnectionDto.UserConnection.Round,
                PlayerPoints = cardGameUsersConnectionDto.UserConnection.RoundPoints,
                EnemyPoints = cardGameUsersConnectionDto.EnemyUserConnection.RoundPoints,
                DamageDoneToEnemy = 0,
                DamageDoneToPlayer = 0,
                EnemyAttack = 0,
                EnemyDefense = 0,
                PlayerAttack = 0,
                PlayerDefense = 0,
                PlayerPlayedCards = [.. playerPlayedCards],
                EnemyPlayedCards = [.. enemyPlayedCards],
                IsEndRound = false,
                IsPlayerWinner = false,
                IsEnemyWinner = false
            };

            Dictionary<CardGameCardAbility, int> playerSpecialCards = GenerateSpecialCardsDictionary(roundResults.PlayerPlayedCards);
            Dictionary<CardGameCardAbility, int> enemySpecialCards = GenerateSpecialCardsDictionary(roundResults.EnemyPlayedCards);

            roundResults.PlayerPlayedCards = DoubleAttackDefense(roundResults.PlayerPlayedCards, playerSpecialCards, CardGameCardAbility.DoubleCardAttackValue);
            roundResults.PlayerPlayedCards = DoubleAttackDefense(roundResults.PlayerPlayedCards, playerSpecialCards, CardGameCardAbility.DoubleCardDefenseValue);
            roundResults.EnemyPlayedCards = DoubleAttackDefense(roundResults.EnemyPlayedCards, enemySpecialCards, CardGameCardAbility.DoubleCardAttackValue);
            roundResults.EnemyPlayedCards = DoubleAttackDefense(roundResults.EnemyPlayedCards, enemySpecialCards, CardGameCardAbility.DoubleCardDefenseValue);

            if (playerSpecialCards.ContainsKey(CardGameCardAbility.AddAttackToAll) || playerSpecialCards.ContainsKey(CardGameCardAbility.AddDefenseToAll))
            {
                roundResults.PlayerPlayedCards = BuffAttackOrDefenseToAllCards(roundResults.PlayerPlayedCards, playerSpecialCards);
            }

            if (enemySpecialCards.ContainsKey(CardGameCardAbility.AddAttackToAll) || enemySpecialCards.ContainsKey(CardGameCardAbility.AddDefenseToAll))
            {
                roundResults.EnemyPlayedCards = BuffAttackOrDefenseToAllCards(roundResults.EnemyPlayedCards, enemySpecialCards);
            }

            if (enemySpecialCards.ContainsKey(CardGameCardAbility.ReduceAttackToAll) || enemySpecialCards.ContainsKey(CardGameCardAbility.ReduceDefenseToAll))
            {
                roundResults.PlayerPlayedCards = DebuffAttackOrDefenseToAllCards(roundResults.PlayerPlayedCards, enemySpecialCards);
            }

            if (playerSpecialCards.ContainsKey(CardGameCardAbility.ReduceAttackToAll) || playerSpecialCards.ContainsKey(CardGameCardAbility.ReduceDefenseToAll))
            {
                roundResults.EnemyPlayedCards = DebuffAttackOrDefenseToAllCards(roundResults.EnemyPlayedCards, playerSpecialCards);
            }

            roundResults.PlayerPlayedCards = DisableStrongestCard(roundResults.PlayerPlayedCards, enemySpecialCards);
            roundResults.EnemyPlayedCards = DisableStrongestCard(roundResults.EnemyPlayedCards, playerSpecialCards);

            roundResults.PlayerPlayedCards.ForEach(card =>
            {
                if (card.WasDestroyedByAnotherCard != true)
                {
                    roundResults.PlayerAttack += card.AttackValue;
                    roundResults.PlayerDefense += card.DefenseValue;
                }
            });

            roundResults.EnemyPlayedCards.ForEach(card =>
            {
                if (card.WasDestroyedByAnotherCard != true)
                {
                    roundResults.EnemyAttack += card.AttackValue;
                    roundResults.EnemyDefense += card.DefenseValue;
                }
            });

            roundResults.DamageDoneToPlayer = roundResults.EnemyAttack - roundResults.PlayerDefense > 0 ? roundResults.EnemyAttack - roundResults.PlayerDefense : 0;
            roundResults.DamageDoneToEnemy = roundResults.PlayerAttack - roundResults.EnemyDefense > 0 ? roundResults.PlayerAttack - roundResults.EnemyDefense : 0;

            roundResults.PlayerHitpoints += -roundResults.DamageDoneToPlayer;
            roundResults.EnemyHitpoints += -roundResults.DamageDoneToEnemy;

            using var transaction = _cardGameGameRepository.CardGameGameRepositoryTransaction();
            try
            {
                var playerCardIds = playerPlayedCards.Select(c => c.Id);
                var enemyCardIds = enemyPlayedCards.Select(c => c.Id);

                await _cardGameGameRepository.UpdatePlayersHPAfterRoundEnds(cardGameUsersConnectionDto, roundResults);
                await _cardGameGameRepository.DeletePlayedCards(cardGameUsersConnectionDto.UserConnection, playerCardIds);
                await _cardGameGameRepository.DeletePlayedCards(cardGameUsersConnectionDto.EnemyUserConnection, enemyCardIds);

                if (cardGameUsersConnectionDto.UserConnection.Round >= CardGameConfig.MaxTurnsToEndRound || roundResults.EnemyHitpoints <= 0 || roundResults.PlayerHitpoints <= 0)
                {
                    await _cardGameGameRepository.DeletePlayerHandCards(cardGameUsersConnectionDto.UserConnection);
                    await _cardGameGameRepository.DeletePlayerHandCards(cardGameUsersConnectionDto.EnemyUserConnection);
                    if (roundResults.PlayerHitpoints > roundResults.EnemyHitpoints || (roundResults.EnemyHitpoints <= 0 && roundResults.EnemyHitpoints > 0))
                    {
                        roundResults.PlayerPoints++;
                        roundResults.IsPlayerWinner = true;
                        await _cardGameGameRepository.AddRoundPoints(cardGameUsersConnectionDto.UserConnection, roundResults.PlayerPoints);
                    }
                    else if (roundResults.PlayerHitpoints < roundResults.EnemyHitpoints || (roundResults.PlayerHitpoints <= 0 && roundResults.EnemyHitpoints > 0))
                    {
                        roundResults.EnemyPoints++;
                        roundResults.IsEnemyWinner = true;
                        await _cardGameGameRepository.AddRoundPoints(cardGameUsersConnectionDto.EnemyUserConnection, roundResults.EnemyPoints);
                    }
                    roundResults.Round = 0;
                    roundResults.IsEndRound = true;
                    await _cardGameGameRepository.ResetPlayersParams(cardGameUsersConnectionDto);
                }

                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw new Exception("TRANSACTION ROLLED BACK");
            }
            return roundResults;
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

        private static Dictionary<CardGameCardAbility, int> GenerateSpecialCardsDictionary(List<CardGameCardDto> playedCards)
        {
            Dictionary<CardGameCardAbility, int> specialCards = [];
            for (int index = 0; index < playedCards.Count; index++)
            {
                var item = playedCards[index];
                if (item.SpecialAbility != 0)
                {
                    specialCards.Add(item.SpecialAbility, index);
                }
            }
            return specialCards;
        }

        private static List<CardGameCardDto> BuffAttackOrDefenseToAllCards(List<CardGameCardDto> playedCards, Dictionary<CardGameCardAbility, int> specialCards)
        {
            return playedCards.Select(card =>
            {
                card.AttackValue = specialCards.ContainsKey(CardGameCardAbility.AddAttackToAll) ? card.AttackValue + 2 : card.AttackValue;
                card.DefenseValue = specialCards.ContainsKey(CardGameCardAbility.AddDefenseToAll) ? card.DefenseValue + 2 : card.DefenseValue;
                return card;
            }).ToList();
        }

        private static List<CardGameCardDto> DebuffAttackOrDefenseToAllCards(List<CardGameCardDto> playedCards, Dictionary<CardGameCardAbility, int> specialCards)
        {
            return playedCards.Select(card =>
            {
                card.AttackValue = specialCards.ContainsKey(CardGameCardAbility.ReduceAttackToAll) ? card.AttackValue - 2 > 0 ? card.AttackValue - 2 : 0 : card.AttackValue;
                card.DefenseValue = specialCards.ContainsKey(CardGameCardAbility.ReduceDefenseToAll) ? card.DefenseValue - 2 > 0 ? card.DefenseValue - 2 : 0 : card.DefenseValue;
                return card;
            }).ToList();
        }

        private static List<CardGameCardDto> DisableStrongestAttackCard(List<CardGameCardDto> playedCards)
        {
            var cardWithHighestValue = playedCards.OrderByDescending(i => i.AttackValue).First();
            return playedCards.Select(item =>
            {
                if (item.AttackValue == cardWithHighestValue.AttackValue)
                {
                    item.WasDestroyedByAnotherCard = true;
                }
                return item;
            }).ToList();
        }

        private static List<CardGameCardDto> DisableStrongestDefenseCard(List<CardGameCardDto> playedCards)
        {
            var cardWithHighestValue = playedCards.OrderByDescending(i => i.DefenseValue).First();
            return playedCards.Select(item =>
            {
                if (item.DefenseValue == cardWithHighestValue.DefenseValue)
                {
                    item.WasDestroyedByAnotherCard = true;
                }
                return item;
            }).ToList();
        }

        private static List<CardGameCardDto> DisableStrongestCard(List<CardGameCardDto> playedCards, Dictionary<CardGameCardAbility, int> specialCards)
        {
            if (specialCards.ContainsKey(CardGameCardAbility.DisableStrongestAttackCard))
            {
                playedCards = DisableStrongestAttackCard(playedCards);
            }
            if (specialCards.ContainsKey(CardGameCardAbility.DisableStrongestDefenseCard))
            {
                playedCards = DisableStrongestDefenseCard(playedCards);
            }
            return playedCards;
        }

        private static List<CardGameCardDto> DoubleAttackDefense(List<CardGameCardDto> playedCards, Dictionary<CardGameCardAbility, int> specialCards, CardGameCardAbility cardGameCardAbility)
        {
            if (specialCards.TryGetValue(cardGameCardAbility, out int cardIndex))
            {
                var leftCard = cardIndex - 1 < 0 ? null : playedCards[cardIndex - 1];
                var rightCard = cardIndex + 1 > playedCards.Count - 1 ? null : playedCards[cardIndex + 1];
                if (leftCard != null)
                {
                    if (CardGameCardAbility.DoubleCardAttackValue == cardGameCardAbility)
                    {
                        leftCard.AttackValue *= 2;
                    }
                    else
                    {
                        leftCard.DefenseValue *= 2;
                    }
                }
                if (rightCard != null)
                {
                    if (CardGameCardAbility.DoubleCardAttackValue == cardGameCardAbility)
                    {
                        rightCard.AttackValue *= 2;
                    }
                    else
                    {
                        rightCard.DefenseValue *= 2;
                    }
                }
            }
            return playedCards;
        }
    }
}

