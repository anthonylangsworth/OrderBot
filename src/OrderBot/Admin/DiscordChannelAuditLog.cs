using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using OrderBot.Core;

namespace OrderBot.Admin
{
    /// <summary>
    /// An audit log that writes to a Discord channel.
    /// </summary>
    public class DiscordChannelAuditLog : IDiscordAuditLog
    {
        /// <summary>
        /// Create a new <see cref="Audit"/>.
        /// </summary>
        /// <param name="logger">
        /// Also log audit events.
        /// </param>
        public DiscordChannelAuditLog(SocketInteractionContext context,
            ILogger<DiscordChannelAuditLog> logger)
        {
            Context = context;
            Logger = logger;
        }

        public SocketInteractionContext Context { get; }

        /// <summary>
        /// Also log audit events.
        /// </summary>
        public ILogger<DiscordChannelAuditLog> Logger { get; }

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
        public void Audit(DiscordGuild discordGuild, string message)
        {
            if (Context.Guild.GetChannel(discordGuild.AuditChannel ?? 0) is SocketTextChannel auditChannel)
            {
                string displayName = Context.Guild.GetUser(Context.User.Id).DisplayName;
                auditChannel.SendMessageAsync($"{displayName}: {message}").GetAwaiter().GetResult();
                Logger.LogInformation("Audit message for '{discordGuildName}': {user}: {message}",
                    discordGuild.Name, displayName, message);
            }
        }
    }
}
