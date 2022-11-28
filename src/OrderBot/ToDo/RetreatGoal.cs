using OrderBot.Core;

namespace OrderBot.ToDo;

/// <summary>
/// Retreat the minor faction by lowering its influence.
/// </summary>
internal class RetreatGoal : Goal
{
    /// <summary>
    /// Singleton.
    /// </summary>
    public static readonly RetreatGoal Instance = new();

    /// <summary>
    /// Prevent instantiation.
    /// </summary>
    private RetreatGoal()
        : base("Retreat", $"Reduce influence below {Math.Round(InfluenceThreshold * 100, 0)}% and keep it there.")
    {
        // Do nothing
    }

    /// <summary>
    /// The influence threshold to force a retreat.
    /// </summary>
    public static double InfluenceThreshold => 0.05;

    /// <inheritdoc/>
    public override IEnumerable<Suggestion> GetSuggestions(Presence presence,
        IReadOnlySet<Presence> systemPresences, IReadOnlySet<Conflict> systemConflicts)
    {
        CheckAddActionsPreconditions(presence, systemPresences, systemConflicts);

        ConflictSuggestion? conflictSuggestion = GetConflict(systemConflicts,
            c => Fight.Against(presence.MinorFaction, c));
        if (conflictSuggestion != null)
        {
            yield return conflictSuggestion;
        }
        else
        {
            if (presence.Influence >= InfluenceThreshold)
            {
                yield return new InfluenceSuggestion(
                    presence.StarSystem, presence.MinorFaction, false, presence.Influence);
            }
        }
    }
}
