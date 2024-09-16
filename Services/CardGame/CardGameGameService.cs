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

        public async Task<List<CardGameCardDto>?> AddCardsToCardGameHand(List<int> cardGameIds, CardGameUsersConnectionDto? cardGameUsersConnectionDto)
        {
            if (cardGameUsersConnectionDto?.UserConnection == null)
            {
                throw new NotFoundException("CONNECTED USER NOT FOUND");
            }

            // @TODO const z backendu
            if (cardGameIds.Count != 5)
            {
                throw new BadHttpRequestException("NUMBER OF CARD IDS MUST BE EQUAL TO 10");
            }

            var cardGameCards = await _cardGameCardRepository.GetCardsByIds(cardGameIds);
            // @TODO const z backendu
            if (cardGameCards.Count != 5)
            {
                throw new BadHttpRequestException("SOME CARD IDS DOESN'T EXISTS");
            }

            await _cardGameGameRepository.AddCardsToCardGameHand(cardGameUsersConnectionDto.UserConnection, cardGameCards);
            var cardGameHand = await _cardGameGameRepository.GetUserCardGameHand(cardGameUsersConnectionDto.UserConnection);

            await _hubContext.Clients.Client(cardGameUsersConnectionDto.EnemyUserConnection.ConnectionId).SendRound(1);

            return cardGameHand;
        }

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
                await _cardGameGameRepository.DeletePlayedCardsFromHand(cardGameUsersConnectionDto.UserConnection, cardGameCard);

                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw new Exception("TRANSACTION ROLLED BACK");
            }
        }
    }
}