using OrderBot.Core;

namespace OrderBot.ToDo;

/// <summary>
/// Keep the minor faction in the system but do not control it.
/// </summary>
internal class MaintainGoal : Goal
{
    /// <summary>
    /// Singleton.
    /// </summary>
    public static readonly MaintainGoal Instance = new();

    /// <summary>
    /// Prevent instantiation.
    /// </summary>
    private MaintainGoal()
        : base("Maintain", $"Keep influence above {Math.Round(LowerInfluenceThreshold * 100, 0)}% and below the controlling minor faction.")
    {
        // Do nothing
    }

    /// <summary>
    /// Do Pro work if the influence falls below this value. This is intentionally
    /// higher than <see cref="RetreatGoal.InfluenceThreshold"/>.
    /// </summary>
    public static double LowerInfluenceThreshold => 0.08;

    /// <summary>
    /// Do Anti work if the influence is within this of the controlling
    /// minor faction's influence.
    /// </summary>
    public static double MaxInfuenceGap => 0.03;

    /// </inheritdoc>
    public override IEnumerable<Suggestion> GetSuggestions(Presence presence,
        IReadOnlySet<Presence> systemPresences, IReadOnlySet<Conflict> systemConflicts)
    {
        CheckAddActionsPreconditions(presence, systemPresences, systemConflicts);

        Presence controllingMinorFaction = GetControllingPresence(systemPresences);
        if (controllingMinorFaction.MinorFaction == presence.MinorFaction)
        {
            if (systemPresences.Count > 1)
            {
                ConflictSuggestion? conflictSuggestion = GetConflict(systemConflicts,
                    c => Fight.Against(presence.MinorFaction, c, SuggestionDescriptions.AvoidControl));
                if (conflictSuggestion != null)
                {
                    yield return conflictSuggestion;
                }
                else
                {
                    yield return new InfluenceSuggestion(
                        presence.StarSystem, presence.MinorFaction, false, presence.Influence,
                        SuggestionDescriptions.AvoidControl);
                }
            }
        }
        else
        {
            ConflictSuggestion? conflictSuggestion = GetConflict(systemConflicts,
                c => Fight.Between(controllingMinorFaction.MinorFaction, presence.MinorFaction, c,
                SuggestionDescriptions.AvoidControl),
                c => Fight.For(presence.MinorFaction, c));
            if (conflictSuggestion != null)
            {
                yield return conflictSuggestion;
            }
            else
            {
                double maxInfluence = controllingMinorFaction.Influence - MaxInfuenceGap;
                if (presence.Influence < LowerInfluenceThreshold)
                {
                    yield return new InfluenceSuggestion(
                        presence.StarSystem, presence.MinorFaction, true, presence.Influence);
                }
                else if (presence.Influence > maxInfluence)
                {
                    yield return new InfluenceSuggestion(
                        presence.StarSystem, presence.MinorFaction, false, presence.Influence);
                }
            }
        }
    }
}
