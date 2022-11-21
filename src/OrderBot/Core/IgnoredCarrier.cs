namespace OrderBot.Core;

public record IgnoredCarrier
{
    public int Id { get; init; }
    public DiscordGuild DiscordGuild { get; init; } = null!;
    public Carrier Carrier { get; init; } = null!;

    public override string ToString()
    {
        return $"{DiscordGuild} ignores {Carrier}";
    }
}
