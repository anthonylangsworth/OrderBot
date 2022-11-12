namespace OrderBot.ToDo;

internal record EddnConflict
{
    public EddnConflictFaction Faction1 { get; init; } = null!;
    public EddnConflictFaction Faction2 { get; init; } = null!;
    public string? Status { get; set; } = null;
    public string WarType { get; init; } = null!;
}
