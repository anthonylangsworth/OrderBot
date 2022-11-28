using OrderBot.Core;

namespace OrderBot.ToDo;

/// <summary>
/// Ensure the minor faction controls (has the highest influence) the system.
/// </summary>
internal class ControlGoal : Goal
{
    /// <summary>
    /// Singleton.
    /// </summary>
    public static readonly ControlGoal Instance = new();

    /// <summary>
    /// Prevent instantiation.
    /// </summary>
    private ControlGoal()
        : base("Control", $"Have the highest influence. Keep influence between {Math.Round(LowerInfluenceThreshold * 100, 0)}% and {Math.Round(UpperInfluenceThreshold * 100, 0)}%.")
    {
        // Do nothing
    }

    /// <summary>
    /// Work for this minor faction if the influence drops below this level.
    /// </summary>
    public static double LowerInfluenceThreshold => 0.55;

    /// <summary>
    /// Work against this minor faction if the influence raises above this level.
    /// </summary>
    public static double UpperInfluenceThreshold => 0.65;

    /// <inheritdoc/>
    public override IEnumerable<Suggestion> GetSuggestions(Presence presence,
        IReadOnlySet<Presence> systemPresences, IReadOnlySet<Conflict> systemConflicts)
    {
        CheckAddActionsPreconditions(presence, systemPresences, systemConflicts);

        ConflictSuggestion? conflictSuggestion =
            GetConflict(systemConflicts, c => Fight.For(presence.MinorFaction, c));
        if (conflictSuggestion != null)
        {
            yield return conflictSuggestion;
        }
        else
        {
            if (presence.Influence < LowerInfluenceThreshold)
            {
                yield return new InfluenceSuggestion(
                    presence.StarSystem, presence.MinorFaction, true, presence.Influence);
            }
            else if (presence.Influence > UpperInfluenceThreshold)
            {
                yield return new InfluenceSuggestion(
                    presence.StarSystem, presence.MinorFaction, false, presence.Influence);
            }
        }

        // Security only applies for the controlling minor faction
        if (presence == GetControllingPresence(systemPresences)
            && presence.SecurityLevel == SecurityLevel.Low)
        {
            yield return new SecuritySuggestion(presence.StarSystem, presence.SecurityLevel);
        }
    }
}
