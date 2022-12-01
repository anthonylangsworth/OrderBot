namespace OrderBot.Core;

/// <summary>
/// An unknown star system was specified.
/// </summary>
internal class UnknownStarSystemException : Exception
{
    public UnknownStarSystemException(string starSystemName)
        : base($"Unknown star system {starSystemName}")
    {
        StarSystemName = starSystemName;
    }

    public string StarSystemName { get; }
}
