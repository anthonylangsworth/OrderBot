using OrderBot.Core;

namespace OrderBot.ToDo
{
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
            : base("Expand", "Expand out of this system.")
        {
            // Do nothing
        }

        /// <summary>
        /// Work for this minor faction until influence reaches this level.
        /// </summary>
        public static double InfluenceThreshold => 0.75;

        /// <inheritdoc/>
        public override IEnumerable<Suggestion> GetSuggestions(Presence starSystemMinorFaction,
            IReadOnlySet<Presence> systemPresences, IReadOnlySet<Conflict> systemConflicts)
        {
            CheckAddActionsPreconditions(starSystemMinorFaction, systemPresences, systemConflicts);

            ConflictSuggestion? conflictSuggestion = GetConflict(systemConflicts,
                c => Fight.For(starSystemMinorFaction.MinorFaction, c));
            if (conflictSuggestion != null)
            {
                yield return conflictSuggestion;
            }
            else
            {
                if (starSystemMinorFaction.Influence < InfluenceThreshold)
                {
                    yield return new InfluenceSuggestion
                    {
                        StarSystem = starSystemMinorFaction.StarSystem,
                        Influence = starSystemMinorFaction.Influence,
                        Pro = true
                    };
                }
            }
        }
    }
}
