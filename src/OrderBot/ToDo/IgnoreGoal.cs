using OrderBot.Core;

namespace OrderBot.ToDo;

internal class IgnoreGoal : Goal
{
    /// <summary>
    /// Singleton.
    /// </summary>
    public static readonly IgnoreGoal Instance = new();

    /// <summary>
    /// Prevent instantiation.
    /// </summary>
    private IgnoreGoal()
        : base("Ignore", "Never suggest activity.")
    {
        // Do nothing
    }

    /// <inheritdoc/>
    public override IEnumerable<Suggestion> GetSuggestions(Presence starSystemMinorFaction,
        IReadOnlySet<Presence> systemPresences, IReadOnlySet<Conflict> systemConflicts)
    {
        CheckAddActionsPreconditions(starSystemMinorFaction, systemPresences, systemConflicts);

        // Do nothing

        return Array.Empty<Suggestion>();
    }
}
