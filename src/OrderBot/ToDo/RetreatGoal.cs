using OrderBot.Core;

namespace OrderBot.ToDo
{
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
            : base("Retreat", $"Retreat from the system by reducing influence below {Math.Round(InfluenceThreshold * 100, 0)}% and keeping it there.")
        {
            // Do nothing
        }

        /// <summary>
        /// The influence threshold to force a retreat.
        /// </summary>
        public static double InfluenceThreshold => 0.05;

        /// <inheritdoc/>
        public override void AddSuggestions(StarSystemMinorFaction starSystemMinorFaction,
            IReadOnlySet<StarSystemMinorFaction> systemBgsData, IReadOnlySet<Conflict> systemConflicts,
            ToDoList toDoList)
        {
            CheckAddActionsPreconditions(starSystemMinorFaction, systemBgsData, systemConflicts);

            if (!AddConflicts(starSystemMinorFaction, false, systemConflicts, toDoList))
            {
                if (starSystemMinorFaction.Influence >= InfluenceThreshold)
                {
                    toDoList.Anti.Add(new() { StarSystem = starSystemMinorFaction.StarSystem, Influence = starSystemMinorFaction.Influence });
                }
            }
        }
    }
}
