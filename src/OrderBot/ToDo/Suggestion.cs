using OrderBot.Core;

namespace OrderBot.ToDo;

public abstract record Suggestion
{
    public StarSystem StarSystem { get; init; } = null!;
    public string? Description { get; set; } = null;
}
