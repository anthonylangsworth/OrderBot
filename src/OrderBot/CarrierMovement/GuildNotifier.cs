using Discord;
using OrderBot.Discord;

namespace OrderBot.CarrierMovement
{
    internal class GuildNotifier : IDisposable
    {
        Dictionary<ulong, TextChannelWriter?> _guildTextChannelWriters = new();

        public GuildNotifier()
        {
            _guildTextChannelWriters = new();
        }

        public void Dispose()
        {
            foreach (TextChannelWriter? textChannelWriter in _guildTextChannelWriters.Values)
            {
                textChannelWriter?.Dispose();
            }
        }

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

            if (textChannelWriter != null)
            {
                textChannelWriter.WriteLine(message);
            }
        }
    }
}
