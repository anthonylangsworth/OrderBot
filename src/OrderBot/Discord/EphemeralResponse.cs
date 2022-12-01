using Discord;
using Discord.Interactions;

namespace OrderBot.Discord;

/// <summary>
/// Used by <see cref="BotHostedService"/> and command modules
/// to indicate success or failure clearly to the user.
/// </summary>
internal class EphemeralResponse
{
    public readonly static string ErrorPrefix = "**Error**: ";
    public readonly static string SuccessPrefix = "**Success**: ";

    public EphemeralResponse(SocketInteractionContext context)
    {
        context.Interaction.DeferAsync(ephemeral: true).GetAwaiter().GetResult();
    }

    public async Task Success(InteractionContext context, string message)
    {
        await Write(context, SuccessPrefix, message);
    }

    public async Task Error(InteractionContext context, string message)
    {
        await Write(context, ErrorPrefix, message);
    }

    public async Task Information(InteractionContext context, string message)
    {
        await Write(context, string.Empty, message);
    }

    public async Task Write(InteractionContext context, string prefix, string message)
    {
        await context.Interaction.FollowupAsync(
                text: Limit($"{prefix}{message}"),
                ephemeral: true
            );
    }

    protected static string Limit(string message, int maxLength = DiscordConfig.MaxMessageSize)
    {
        return message.Length > maxLength
            ? message[..maxLength]
            : message;
    }
}
