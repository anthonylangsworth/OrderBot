using Discord;
using Discord.Interactions;
using OrderBot.Discord;

namespace OrderBot.Admin
{
    /// <summary>
    /// An audit log that writes to a Discord channel.
    /// </summary>
    public class DiscordChannelAuditLogger : IAuditLogger
    {
        private bool _disposedValue;
        private readonly DiscordChannelStream _discordChannelStream;
        private readonly BufferedStream _bufferedStream;
        private readonly StreamWriter _streamWriter;

        /// <summary>
        /// Create a new <see cref="DiscordChannelAuditLogger"/>.
        /// </summary>
        /// <param name="channelId">
        /// The channel ID to receive audit messages.
        /// </param>
        /// <param name="guildName">
        /// The name of the Discord guild.
        /// </param>
        /// <param name="userName">
        /// The user naem to write messages for.
        /// </param>
        /// <param name="logger">
        /// Also log audit events.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Either <paramref name="context"/> is invalid or <paramref name="channelId"/> is not a valid channel.
        /// </exception>
        public DiscordChannelAuditLogger(SocketInteractionContext context, ulong channelId, string guildName, string userName)
        {
            if (context.Guild.GetChannel(channelId) is not ITextChannel textChannel)
            {
                throw new ArgumentException(
                    $"Either {nameof(context)} is invalid or {nameof(channelId)} {channelId} is not a valid channel");
            }
            _discordChannelStream = new DiscordChannelStream(textChannel);
            _bufferedStream = new BufferedStream(_discordChannelStream, DiscordConfig.MaxMessageSize);
            _streamWriter = new StreamWriter(_bufferedStream);
            GuildName = guildName;
            UserName = userName;
        }
        ~DiscordChannelAuditLogger()
        {
            Dispose(disposing: false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                _streamWriter.Flush();
                _streamWriter.Dispose();
                _bufferedStream.Dispose();
                _discordChannelStream.Dispose();
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// The name of the Discord guild to audit messages for.
        /// </summary>
        public string GuildName { get; }

        /// <summary>
        /// The user whose actions are being audited.
        /// </summary>
        public string UserName { get; }

        /// <inheritdoc/>
        public void Audit(string message)
        {
            _streamWriter.WriteLine($"{UserName}: {message}");
        }
    }
}
