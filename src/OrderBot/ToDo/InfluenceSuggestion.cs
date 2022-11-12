namespace OrderBot.ToDo;

public record InfluenceSuggestion : Suggestion, IEquatable<InfluenceSuggestion?>
{
    public double Influence { get; set; }
    public bool Pro { get; set; }
}
