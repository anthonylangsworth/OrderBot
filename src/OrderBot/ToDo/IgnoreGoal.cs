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
            IReadOnlySet<Presence> systemBgsData, IReadOnlySet<Conflict> systemConflicts,
            ToDoList toDoList)
        {
            CheckAddActionsPreconditions(starSystemMinorFaction, systemBgsData, systemConflicts);

            // Do nothing
        }
    }
}
