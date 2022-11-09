using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using OrderBot.Core;
using static OrderBot.CarrierMovement.AdminCommandsModule;

namespace OrderBot.Admin
{
    /// <summary>
    /// An audit log that writes to a channel.
    /// </summary>
    public class DiscordChannelAuditLog
    {
        /// <summary>
        /// Create a new <see cref="DiscordChannelAuditLog"/>.
        /// </summary>
        /// <param name="logger">
        /// Also log audit events.
        /// </param>
        public DiscordChannelAuditLog(ILogger<AuditChannel> logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// Also log audit events.
        /// </summary>
        public ILogger<AuditChannel> Logger { get; }

        /// <summary>
        /// Write an audit message for the given <see cref="DiscordGuild"/>.
        /// </summary>
        /// <param name="context">
        /// The <see cref="SocketInteractionContext"/> for the interaction.
        /// </param>
        /// <param name="discordGuild">
        /// The <see cref="DiscordGuild"/> to get the audit channel for.
        /// </param>
        /// <param name="message">
        /// The message to audit.
        /// </param>
        public async void AuditAsync(SocketInteractionContext context, DiscordGuild discordGuild, string message)
        {
            if (context.Guild.GetChannel(discordGuild.AuditChannel ?? 0) is SocketTextChannel auditChannel)
            {
                await auditChannel.SendMessageAsync($"{context.User.Username}: {message}");
                Logger.LogInformation("Audit message for '{discordGuildName}': {user}: {message}",
                    discordGuild.Name, context.User.Username, message);
            }
        }
    }
}
