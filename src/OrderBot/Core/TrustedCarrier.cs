namespace OrderBot.Core
{
    internal class TrustedCarrier
    {
        public int Id { get; init; }
        public DiscordGuild DiscordGuild { get; init; } = null!;
        public Carrier Carrier { get; init; } = null!;
    }
}
