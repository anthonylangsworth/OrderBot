using System.Runtime.Serialization;

namespace OrderBot.Core;

/// <summary>
/// Cannot generate suggestions without a supported minor faction.
/// </summary>
[Serializable]
internal class NoSupportedMinorFactionException : Exception
{
    public NoSupportedMinorFactionException(ulong guildId, Exception innerException)
        : base($"Guild with ID {guildId} has no supported minor faction", innerException)
    {
        GuildId = guildId;
    }

    protected NoSupportedMinorFactionException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        throw new NotImplementedException();
    }

    public ulong GuildId { get; }
}
