using CsvHelper;
using Discord;
using Discord.Interactions;
using System.Globalization;

namespace OrderBot.Discord;

/// <summary>
/// Provide standard user responses to slash (Application) commands
/// used with a <see cref="SocketInteractionContext"/>.
/// </summary>
/// <seealso cref="BotHostedService"/>
internal class EphemeralResponse
{
    /// <summary>
    /// Create a new <see cref="EphemeralResponse"/>.
    /// </summary>
    /// <param name="context">
    /// The interaction to respond to.
    /// </param>
    public EphemeralResponse(SocketInteractionContext context)
    {
        Context = context;
        Context.Interaction.DeferAsync(ephemeral: true).GetAwaiter().GetResult();
    }

    /// <summary>
    /// The context to respond to.
    /// </summary>
    protected SocketInteractionContext Context { get; }
    protected readonly static string ErrorPrefix = "**Error**: ";
    protected readonly static string SuccessPrefix = "**Success**: ";

    /// <summary>
    /// Inform the user of a successful change.
    /// </summary>
    /// <param name="message">
    /// The message. It should describe what changed, referencing
    /// relevant parameters.
    /// </param>
    /// <seealso cref="Error"/>
    /// <seealso cref="Information"/>
    public async Task Success(string message)
    {
        await Write(SuccessPrefix, message);
    }

    /// <summary>
    /// Send a file to the user.
    /// </summary>
    /// <param name="file">
    /// The file contents.
    /// </param>
    /// <param name="fileName">
    /// The file name.
    /// </param>
    public async Task File(string file, string fileName)
    {
        using MemoryStream memoryStream = new();
        using StreamWriter streamWriter = new(memoryStream);
        using CsvWriter csvWriter = new(streamWriter, CultureInfo.InvariantCulture);
        await csvWriter.WriteRecordsAsync(file);
        await csvWriter.FlushAsync();
        memoryStream.Seek(0, SeekOrigin.Begin);
        await Context.Interaction.FollowupWithFileAsync(
            fileStream: memoryStream,
            fileName: fileName,
            ephemeral: true
        );
    }

    /// <summary>
    /// Inform the user of an error.
    /// </summary>
    /// <remarks>
    /// The gaol is to provide more informative and actionable error messages.
    /// </remarks>
    /// <param name="what">
    /// Describe the error.
    /// </param>
    /// <param name="why">
    /// Describe the actions the user took that caused the error or, if it is internal, say so.
    /// </param>
    /// <param name="state">
    /// Describe the system state. For example, were any changes saved?
    /// </param>
    /// <param name="fix">
    /// Describe how to fix the error, such as retrying with the correct arguments.
    /// </param>
    public async Task Error(string what, string why, string state, string fix)
    {
        await Write(ErrorPrefix, $"{what} {why} {state} {fix}");
    }

    /// <summary>
    /// Respond to a request for information.
    /// </summary>
    /// <param name="message">
    /// The information.
    /// </param>
    public async Task Information(string message)
    {
        await Write(string.Empty, message);
    }

    protected async Task Write(string prefix, string message)
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
