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
        public override void AddSuggestions(StarSystemMinorFaction starSystemMinorFaction,
            IReadOnlySet<StarSystemMinorFaction> systemBgsData, IReadOnlySet<Conflict> conflicts,
            ToDoList toDoList)
        {
            CheckAddActionsPreconditions(starSystemMinorFaction, systemBgsData, conflicts);

            // Do nothing
        }
    }
}
