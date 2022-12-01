namespace OrderBot.ToDo;

/// <summary>
/// An unknown minor faction was specified.
/// </summary>
internal class UnknownMinorFactionException : Exception
{
    public UnknownMinorFactionException(string minorFactionName)
        : base($"Unknown minor faction {minorFactionName}")
    {
        MinorFactionName = minorFactionName;
    }

    public string MinorFactionName { get; }
}
