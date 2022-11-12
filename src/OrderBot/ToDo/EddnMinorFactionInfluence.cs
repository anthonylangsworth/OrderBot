namespace OrderBot.ToDo;

/// <summary>
/// A minor faction's influence and states in a star system.
/// </summary>
internal record EddnMinorFactionInfluence
{
    public string MinorFaction { init; get; } = null!;
    public double Influence { init; get; }
    public IReadOnlyList<string> States { init; get; } = null!;
}
