using Discord;
using OrderBot.Discord;

namespace OrderBot.CarrierMovement
{
    /// <summary>
    /// Abstract away writing to per-guild channels by <see cref="CarrierMovementMessageProcessor"/>.
    /// </summary>
    internal class GuildNotifier : IDisposable
    {
        private readonly Dictionary<ulong, TextChannelWriter?> _guildTextChannelWriters = new();

        /// <summary>
        /// Create a new <see cref="GuildNotifier"/>.
        /// </summary>
        public GuildNotifier()
        {
            _guildTextChannelWriters = new();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            foreach (TextChannelWriter? textChannelWriter in _guildTextChannelWriters.Values)
            {
                textChannelWriter?.Dispose();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="getTextChannel"></param>
        /// <param name="channelId"></param>
        /// <param name="message"></param>
        public void NotifyGuild(Func<ulong, ITextChannel> getTextChannel, ulong? channelId, string message)
        {
            if (!_guildTextChannelWriters.TryGetValue(channelId ?? 0, out TextChannelWriter? textChannelWriter))
            {
                ITextChannel? textChannel = getTextChannel(channelId ?? 0);
                if (textChannel != null)
                {
                    textChannelWriter = new TextChannelWriter(textChannel);
                    _guildTextChannelWriters.Add(channelId ?? 0, textChannelWriter);
                }
                else
                {
                    _guildTextChannelWriters.Add(channelId ?? 0, null);
                }
            }

            textChannelWriter?.WriteLine(message);
        }
    }
}
