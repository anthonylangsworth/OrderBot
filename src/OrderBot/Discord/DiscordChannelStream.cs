using Discord;
using System.Text;

namespace OrderBot.Discord
{
    /// <summary>
    /// A <see cref="Stream"/> that wraps a <see cref="ITextChannel"/>. It only allows writes.
    /// </summary>
    public class DiscordChannelStream : Stream
    {
        /// <summary>
        /// Create a new <see cref="DiscordChannelStream"/>.
        /// </summary>
        /// <param name="textChannel">
        /// The Discord channel to write to. The caller needs `Send Messages` permission on this channel.
        /// </param>
        public DiscordChannelStream(ITextChannel textChannel)
        {
            TextChannel = textChannel;
        }

        /// <summary>
        /// The Discord channel to write to. The caller needs `Send Messages` permission on this channel.
        /// </summary>
        internal ITextChannel TextChannel { get; }

        /// <inheritdoc/>
        public override bool CanRead => false;

        /// <inheritdoc/>
        public override bool CanSeek => false;

        /// <inheritdoc/>
        public override bool CanWrite => true;

        /// <inheritdoc/>
        public override long Length => throw new NotSupportedException();

        /// <inheritdoc/>
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override void Flush()
        {
            // Do nothing
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            TextChannel.SendMessageAsync(Encoding.UTF8.GetString(buffer, offset, count)).GetAwaiter().GetResult();
        }
    }
}
