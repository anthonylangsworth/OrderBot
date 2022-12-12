namespace OrderBot.Core;

public class Presence
{
    public int Id { get; }
    public StarSystem StarSystem { get; init; } = null!;
    public MinorFaction MinorFaction { get; init; } = null!;
    public double Influence;
    public string? SecurityLevel;
    public List<State> States { get; init; } = new();

    public override string ToString()
    {
        return $"{MinorFaction} in {StarSystem} with inf {Math.Round(Influence * 100, 0)}%";
    }
}
