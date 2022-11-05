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
            : base("Expand", "Expand this minor faction out of this system.")
        {
            // Do nothing
        }

        /// <summary>
        /// Work for this minor faction until influence reaches this level.
        /// </summary>
        public static double InfluenceThreshold => 0.75;

        /// <inheritdoc/>
        public override void AddSuggestions(Presence starSystemMinorFaction,
            IReadOnlySet<Presence> systemBgsData, IReadOnlySet<Conflict> systemConflicts,
            ToDoList toDoList)
        {
            CheckAddActionsPreconditions(starSystemMinorFaction, systemBgsData, systemConflicts);

            if (!AddConflicts(systemConflicts, toDoList,
                c => Fight.For(starSystemMinorFaction.MinorFaction, c)))
            {
                if (starSystemMinorFaction.Influence < InfluenceThreshold)
                {
                    toDoList.Pro.Add(new InfluenceSuggestion { StarSystem = starSystemMinorFaction.StarSystem, Influence = starSystemMinorFaction.Influence });
                }
            }
        }
    }
}
