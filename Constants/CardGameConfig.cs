namespace web_bite_server.Constants
{
    public static class CardGameConfig
    {
        public static int UserHitPoints { get; } = 30;
        public static int MinCardsToAdd { get; } = 1;
        public static int MaxCardsToAdd { get; } = 5;
        public static int MaxCardsInHand { get; } = 10;
        public static int MaxRoundsToEndGame { get; } = 3;
    }
}