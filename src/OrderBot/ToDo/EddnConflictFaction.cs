namespace OrderBot.ToDo;

internal record EddnConflictFaction
{
    public string Name { get; set; } = null!;
    public string Stake { get; set; } = null!;
    public int WonDays { get; set; }
}