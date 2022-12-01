using CsvHelper;
using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;

namespace OrderBot.Discord;

/// <summary>
/// Provide standard user responses to slash (Application) commands
/// used with a <see cref="SocketInteractionContext"/>.
/// </summary>
/// <seealso cref="BotHostedService"/>
public class EphemeralResult
{
    /// <summary>
    /// Create a new <see cref="EphemeralResult"/>.
    /// </summary>
    /// <param name="context">
    /// The interaction to respond to.
    /// </param>
    /// <param name="auditLogger">
    /// Audit here.
    /// </param>
    /// <param name="logger">
    /// Log here.
    /// </param>
    public EphemeralResult(SocketInteractionContext context, IAuditLogger auditLogger, ILogger logger)
    {
        Context = context;
        AuditLogger = auditLogger;
        Logger = logger;

        // Context.Interaction.DeferAsync(ephemeral: true).GetAwaiter().GetResult();
    }

    /// <summary>
    /// The context to respond to.
    /// </summary>
    protected SocketInteractionContext Context { get; }
    protected IAuditLogger AuditLogger { get; }
    protected ILogger Logger { get; }

    // TODO: Make protected
    public readonly static string ErrorPrefix = "**Error**: ";
    public readonly static string SuccessPrefix = "**Success**: ";

    /// <summary>
    /// Inform the user of and log a successful change or action.
    /// </summary>
    /// <param name="message">
    /// The message. It should describe what changed, referencing
    /// relevant parameters. This is also logged.
    /// </param>
    /// <param name="audit">
    /// If <c>true</c>, also write <paramref name="message"/> to the audit log.
    /// If <c>false</c>, do nothing, which is the default.
    /// </param>
    /// <seealso cref="Error"/>
    /// <seealso cref="Information"/>
    public async Task Success(string message, bool audit = false)
    {
        await Write(SuccessPrefix, message);
        if (audit)
        {
            AuditLogger.Audit(message);
        }

#pragma warning disable CA2254
        Logger.LogInformation(message);
#pragma warning restore
    }

    /// <summary>
    /// Send a CSV file to the user. Log the file sent.
    /// </summary>
    /// <param name="records">
    /// The records to write.
    /// </param>
    /// <param name="fileName">
    /// The file name.
    /// </param>
    /// <exception cref="CsvHelperException">
    /// An error occured creating the CSV file.
    /// </exception>
    public async Task CsvFile<T>(IEnumerable<T> records, string fileName)
    {
        using MemoryStream memoryStream = new();
        using StreamWriter streamWriter = new(memoryStream);
        using CsvWriter csvWriter = new(streamWriter, CultureInfo.InvariantCulture);
        await csvWriter.WriteRecordsAsync(records);
        await csvWriter.FlushAsync();
        memoryStream.Seek(0, SeekOrigin.Begin);
        await Context.Interaction.FollowupWithFileAsync(
            fileStream: memoryStream,
            fileName: fileName,
            ephemeral: true
        );

        Logger.LogInformation("CSV file {File} sent", fileName);
    }

    /// <summary>
    /// Send a file to the user. Log the file sent.
    /// </summary>
    /// <param name="records">
    /// The records to write.
    /// </param>
    /// <param name="fileName">
    /// The file name.
    /// </param>
    public async Task File(string file, string fileName)
    {
        using MemoryStream memoryStream = new(Encoding.UTF8.GetBytes(file));
        await Context.Interaction.FollowupWithFileAsync(
            fileStream: memoryStream,
            fileName: fileName,
            ephemeral: true
        );

        Logger.LogInformation("File {File} sent", fileName);
    }

    /// <summary>
    /// Inform the user of an error.
    /// </summary>
    /// <remarks>
    /// The goal is to provide more informative and actionable error messages.
    /// </remarks>
    /// <param name="what">
    /// Describe the error. What could not be completed?
    /// </param>
    /// <param name="why">
    /// Describe the actions the user took that caused the error. This is logged and,
    /// if <paramref name="audit"/> is true, audited.
    /// </param>
    /// <param name="fix">
    /// Describe how to fix the error, such as retrying with the correct arguments.
    /// </param>
    /// <param name="audit">
    /// If <c>true</c>, also write <paramref name="why"/> to the audit log.
    /// If <c>false</c>, do nothing, which is the default.
    /// </param>
    public async Task Error(string what, string why, string fix, bool audit = false)
    {
        StringBuilder message = new();
        foreach (string? s in new[] { what, why, fix })
        {
            if (!string.IsNullOrWhiteSpace(s))
            {
                message.Append(s);
                message.Append(' ');
            }
        }
        await Write(ErrorPrefix, message.ToString().Trim());

        if (audit)
        {
            AuditLogger.Audit(why);
        }
#pragma warning disable CA2254
        Logger.LogWarning(why);
#pragma warning restore
    }

    /// <summary>
    /// Respond to a request for information. <paramref name="message"/> is also logged
    /// if <paramref name="log"/> is true (default).
    /// </summary>
    /// <param name="message">
    /// The information to be sent to the user and logged.
    /// </param>
    /// <param name="log">
    /// If <c>true</c>, message is logged (default). If <c>false</c>, message is not logged.
    /// </param>
    public async Task Information(string message, bool log = true)
    {
        await Write(string.Empty, message);
#pragma warning disable CA2254
        if (log)
        {
            Logger.LogInformation(message);
        }
#pragma warning restore
    }

    /// <summary>
    /// Handle an unknown exception.
    /// </summary>
    /// <param name="ex">
    /// The exception to handle.
    /// </param>
    public async Task Exception(Exception ex)
    {
        await Write("", "The command failed due to an internal error. The error has been logged for review.");
        Logger.LogError(ex, "Unknown error");
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
