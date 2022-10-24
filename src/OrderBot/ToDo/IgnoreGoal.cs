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
        public override void AddActions(StarSystemMinorFaction starSystemMinorFaction,
            IReadOnlyList<StarSystemMinorFaction> systemBgsData, ToDoList toDoList)
        {
            // Do nothing
        }
    }
}
