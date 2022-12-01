using CsvHelper;
using Discord;
using Discord.Interactions;
using System.Globalization;

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
        Context = context;
    }

    protected SocketInteractionContext Context { get; }

    public async Task Success(string message)
    {
        await Write(SuccessPrefix, message);
    }

    public async Task File(string file, string fileName)
    {
        using MemoryStream memoryStream = new();
        using StreamWriter streamWriter = new(memoryStream);
        using CsvWriter csvWriter = new(streamWriter, CultureInfo.InvariantCulture);
        csvWriter.WriteRecords(file);
        csvWriter.Flush();
        memoryStream.Seek(0, SeekOrigin.Begin);
        await Context.Interaction.FollowupWithFileAsync(
            fileStream: memoryStream,
            fileName: fileName,
            ephemeral: true
        );
    }

    public async Task Error(string message)
    {
        await Write(ErrorPrefix, message);
    }

    public async Task Information(string message)
    {
        await Write(string.Empty, message);
    }

    public async Task Write(string prefix, string message)
    {
        await Context.Interaction.FollowupAsync(
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
