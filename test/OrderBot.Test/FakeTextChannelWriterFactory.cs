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
        StringBuilder = new StringBuilder();
    }

    public StringBuilder StringBuilder;

    public override Task<TextWriter> GetWriterAsync(ulong? channelId)
    {
        return Task.FromResult<TextWriter>(new StringWriter(StringBuilder));
    }
}
