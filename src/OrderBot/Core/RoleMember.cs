namespace OrderBot.Core;

public record RoleMember
{
    public int Id { get; }
    public DiscordGuild DiscordGuild { get; init; } = null!;
    public Role Role { get; init; } = null!;
    public ulong MentionableId { get; init; }
}
