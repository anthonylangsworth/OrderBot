namespace OrderBot.Core;

public class State
{
    public int Id { get; }
    public List<Presence> Presence { get; } = new();
    public string Name = null!;

    public override string ToString()
    {
        return Name;
    }

    public static readonly string Retreat = "Retreat";
}
