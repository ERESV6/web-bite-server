using web_bite_server.Dtos.CardGame;
using web_bite_server.Models;

namespace web_bite_server.Mappers
{
    public static class CardGameMapper
    {
        public static CardGameCardDto ToCardGameCardDto(this CardGameCard cardGameCard)
        {
            return new CardGameCardDto
            {
                CardName = cardGameCard.CardName,
                AttackValue = cardGameCard.AttackValue,
                DefenseValue = cardGameCard.DefenseValue,
                Id = cardGameCard.Id,
                Label = cardGameCard.Label,
                SpecialAbility = cardGameCard.SpecialAbility
            };
        }

        public static CardGameCard ToCardGameCard(this CardGameCardDto cardGameCard)
        {
            return new CardGameCard
            {
                Id = cardGameCard.Id,
                CardName = cardGameCard.CardName,
                Label = cardGameCard.Label,
                AttackValue = cardGameCard.AttackValue,
                DefenseValue = cardGameCard.DefenseValue,
                SpecialAbility = cardGameCard.SpecialAbility
            };
        }

        public static CardGameActiveUserDto ToCardGameActiveUserDto(this CardGameConnection cardGameConnection)
        {
            return new CardGameActiveUserDto
            {
                ConnectionId = cardGameConnection.ConnectionId,
                UserName = cardGameConnection?.AppUser?.UserName ?? "",
                IsAvaliable = !(cardGameConnection?.UserToId?.Length > 0 || cardGameConnection?.UserToRequestPendingId?.Length > 0)
            };
        }
    }
}