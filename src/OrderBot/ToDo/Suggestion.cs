using OrderBot.Core;

namespace OrderBot.ToDo;

public abstract record Suggestion(StarSystem StarSystem, string? Description = null)
{
}
