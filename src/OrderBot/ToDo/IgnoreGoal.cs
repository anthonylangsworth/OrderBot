using OrderBot.Core;

namespace OrderBot.ToDo
{
    internal class IgnoreGoal : Goal
    {
        /// <summary>
        /// Singleton.
        /// </summary>
        public static readonly IgnoreGoal Instance = new();

        /// <summary>
        /// Prevent instantiation.
        /// </summary>
        private IgnoreGoal()
            : base("Ignore", $"Never suggested activity.")
        {
            // Do nothing
        }

        /// <inheritdoc/>
        public override void AddSuggestions(Presence starSystemMinorFaction,
            IReadOnlySet<Presence> systemPresences, IReadOnlySet<Conflict> systemConflicts,
            ToDoList toDoList)
        {
            CheckAddActionsPreconditions(starSystemMinorFaction, systemPresences, systemConflicts);

            // Do nothing
        }
    }
}
