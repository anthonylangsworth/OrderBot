using OrderBot.Core;

namespace OrderBot.ToDo
{
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
        public override void AddSuggestions(StarSystemMinorFaction starSystemMinorFaction,
            IReadOnlySet<StarSystemMinorFaction> systemBgsData, IReadOnlySet<Conflict> systemConflicts,
            ToDoList toDoList)
        {
            CheckAddActionsPreconditions(starSystemMinorFaction, systemBgsData, systemConflicts);

            if (!AddConflicts(systemConflicts, toDoList,
                c => FightForOrAgainst(starSystemMinorFaction.MinorFaction, true, c)))
            {
                if (starSystemMinorFaction.Influence < InfluenceThreshold)
                {
                    toDoList.Pro.Add(new InfluenceSuggestion { StarSystem = starSystemMinorFaction.StarSystem, Influence = starSystemMinorFaction.Influence });
                }
            }
        }
    }
}
