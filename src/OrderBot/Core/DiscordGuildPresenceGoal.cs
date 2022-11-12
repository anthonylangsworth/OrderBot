namespace OrderBot.Core;

public record DiscordGuildPresenceGoal
{
    public int Id { get; init; }
    public DiscordGuild DiscordGuild { get; set; } = null!;
    public Presence Presence { get; set; } = null!;
    public string Goal { get; set; } = null!;
}
