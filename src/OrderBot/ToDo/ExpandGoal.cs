using OrderBot.Core;

namespace OrderBot.ToDo;

/// <summary>
/// Expand the minor faction by raising its influence.
/// </summary>
internal class ExpandGoal : Goal
{
    /// <summary>
    /// Singleton.
    /// </summary>
    public static readonly ExpandGoal Instance = new();

    /// <summary>
    /// Prevent instantiation.
    /// </summary>
    private ExpandGoal()
        : base("Expand", $"Increase influence over {Math.Round(InfluenceThreshold * 100, 0)}% and keep it there.")
    {
        // Do nothing
    }

    /// <summary>
    /// Work for this minor faction until influence reaches this level.
    /// </summary>
    public static double InfluenceThreshold => 0.75;

    /// <inheritdoc/>
    public override IEnumerable<Suggestion> GetSuggestions(Presence presence,
        IReadOnlySet<Presence> systemPresences, IReadOnlySet<Conflict> systemConflicts)
    {
        CheckAddActionsPreconditions(presence, systemPresences, systemConflicts);

        ConflictSuggestion? conflictSuggestion = GetConflict(systemConflicts,
            c => Fight.For(presence.MinorFaction, c));
        if (conflictSuggestion != null)
        {
            yield return conflictSuggestion;
        }
        else
        {
            if (presence.Influence < InfluenceThreshold)
            {
                yield return new InfluenceSuggestion(
                    presence.StarSystem, presence.MinorFaction, true, presence.Influence,
                    SuggestionDescriptions.Expanding);
            }
        }
    }
}
