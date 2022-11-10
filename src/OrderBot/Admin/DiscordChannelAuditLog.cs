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
        private bool disposedValue;

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
            AuditMessages = new List<string>();
        }

        public SocketInteractionContext Context { get; }

        /// <summary>
        /// Also log audit events.
        /// </summary>
        public ILogger<DiscordChannelAuditLog> Logger { get; }

        internal List<string> AuditMessages { get; }

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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                //if (Context.Guild.GetChannel(discordGuild.AuditChannel ?? 0) is SocketTextChannel auditChannel)
                //{
                //    foreach (string auditMessage in AuditMessages)
                //    {
                //        auditChannel.SendMessageAsync(auditMessage).GetAwaiter().GetResult();
                //    }
                //}
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~DiscordChannelAuditLog()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
