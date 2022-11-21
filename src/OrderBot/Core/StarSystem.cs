namespace OrderBot.Core;

public class StarSystem
{
    public int Id { get; }
    public string Name { get; init; } = null!;
    public DateTime? LastUpdated { get; set; }

    public override string ToString()
    {
        return Name;
    }
}
