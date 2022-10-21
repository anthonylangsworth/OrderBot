using OrderBot.Core;

namespace OrderBot.ToDo
{
    internal abstract class Goal
    {
        protected Goal(string name, string description)
        {
            Name = name;
            Description = description;
        }

        /// <summary>
        /// The goal's name, as stored in the datbase.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// A human-readable description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Add actions to the for the <paramref name="starSystemMinorFaction"/> to the <paramref name="toDoList"/>.
        /// </summary>
        /// <param name="starSystemMinorFaction">
        /// The star system and minor faction to determine whether there are actions to perform.
        /// </param>
        /// <param name="toDoList">
        /// Add actions to this.
        /// </param>
        public abstract void AddActions(StarSystemMinorFaction starSystemMinorFaction, ToDoList toDoList);
    }
}
