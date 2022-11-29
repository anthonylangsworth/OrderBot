namespace OrderBot.Discord;

/// <summary>
/// Used by <see cref="BotHostedService"/> and command modules
/// to indicate success or failure clearly to the user.
/// </summary>
internal static class MessagePrefix
{
    public readonly static string Error = "**Error**: ";
    public readonly static string Success = "**Success**: ";
}
