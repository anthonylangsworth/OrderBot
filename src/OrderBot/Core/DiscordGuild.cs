﻿namespace OrderBot.Core
{
    public record DiscordGuild
    {
        public int Id { init; get; }
        public ulong GuildId { init; get; }
        public ulong? CarrierMovementChannel { set; get; }
        public ICollection<Carrier> IgnoredCarriers { init; get; } = null!;
    }
}
