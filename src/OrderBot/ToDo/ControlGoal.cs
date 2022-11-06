using OrderBot.Core;

namespace OrderBot.ToDo
{
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
            : base("Control", $"Be the minor faction with the highest influence. Keep influence between {Math.Round(LowerInfluenceThreshold * 100, 0)}% and {Math.Round(UpperInfluenceThreshold * 100, 0)}%.")
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
        public override void AddSuggestions(Presence starSystemMinorFaction,
            IReadOnlySet<Presence> systemPresences, IReadOnlySet<Conflict> systemConflicts, ToDoList toDoList)
        {
            CheckAddActionsPreconditions(starSystemMinorFaction, systemPresences, systemConflicts);

            if (!AddConflicts(systemConflicts, toDoList,
                c => Fight.For(starSystemMinorFaction.MinorFaction, c)))
            {
                if (starSystemMinorFaction.Influence < LowerInfluenceThreshold)
                {
                    toDoList.Suggestions.Add(new InfluenceSuggestion
                    {
                        StarSystem = starSystemMinorFaction.StarSystem,
                        Influence = starSystemMinorFaction.Influence,
                        Pro = true
                    });
                }
                else if (starSystemMinorFaction.Influence > UpperInfluenceThreshold)
                {
                    toDoList.Suggestions.Add(new InfluenceSuggestion
                    {
                        StarSystem = starSystemMinorFaction.StarSystem,
                        Influence = starSystemMinorFaction.Influence,
                        Pro = false
                    });
                }
            }

            // Security only applies for the controlling minor faction
            if (starSystemMinorFaction == GetControllingPresence(systemPresences)
                && starSystemMinorFaction.SecurityLevel == SecurityLevel.Low)
            {
                toDoList.Suggestions.Add(new SecuritySuggestion
                {
                    StarSystem = starSystemMinorFaction.StarSystem,
                    SecurityLevel = starSystemMinorFaction.SecurityLevel
                });
            }
        }
    }
}
