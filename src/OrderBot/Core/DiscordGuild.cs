namespace OrderBot.Core
{
    public record DiscordGuild
    {
        public int Id { init; get; }
        public ulong GuildId { init; get; }
        public string? Name { set; get; }
        public ulong? CarrierMovementChannel { set; get; }
        public ICollection<Carrier> IgnoredCarriers { init; get; } = null!;
        public ICollection<MinorFaction> SupportedMinorFactions { init; get; } = null!;
    }
}
