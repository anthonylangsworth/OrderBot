namespace OrderBot.Core;

public class Role
{
    public int Id { get; }
    public string Name { get; init; } = null!;

    public override string ToString()
    {
        return Name;
    }
}
