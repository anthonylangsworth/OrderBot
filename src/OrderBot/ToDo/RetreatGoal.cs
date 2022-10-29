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
            : base("Retreat", $"Retreat from the system by reducing influence below {Math.Round(Threshold * 100, 0)}% and keeping it there.")
        {
            // Do nothing
        }

        /// <summary>
        /// The influence threshold to force a retreat.
        /// </summary>
        public static double Threshold => 0.05;

        /// <inheritdoc/>
        public override void AddActions(StarSystemMinorFaction starSystemMinorFaction,
            IReadOnlyList<StarSystemMinorFaction> systemBgsData, ToDoList toDoList)
        {
            CheckAddActionsPreconditions(starSystemMinorFaction, systemBgsData);

            toDoList.Anti.Add(new() { StarSystem = starSystemMinorFaction.StarSystem, Influence = starSystemMinorFaction.Influence });

            // TODO: Handle conflicts
        }
    }
}
