namespace OrderBot.ToDo;

public record SecuritySuggestion : Suggestion, IEquatable<SecuritySuggestion?>
{
    public string SecurityLevel { get; set; } = null!;
}
