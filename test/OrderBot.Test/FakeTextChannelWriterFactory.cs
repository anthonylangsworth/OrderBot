using Discord;
using Moq;
using OrderBot.Discord;
using System.Text;

namespace OrderBot.Test;
internal class FakeTextChannelWriterFactory : TextChannelWriterFactory
{
    public FakeTextChannelWriterFactory()
        : base(new Mock<IDiscordClient>().Object)
    {
        ChannelToStringBuilder = new();
    }

    public Dictionary<ulong, StringBuilder> ChannelToStringBuilder;

    public override Task<TextWriter> GetWriterAsync(ulong? channelId)
    {
        if (!ChannelToStringBuilder.TryGetValue(channelId ?? 0, out StringBuilder? stringBuilder))
        {
            stringBuilder = new StringBuilder();
            ChannelToStringBuilder.Add(channelId ?? 0, stringBuilder);
        }
        return Task.FromResult<TextWriter>(new StringWriter(stringBuilder));
    }
}
