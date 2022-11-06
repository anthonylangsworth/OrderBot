namespace OrderBot.ToDo
{
    /// <summary>
    /// A To Do List, containing the various work for commanders to support a minor faction.
    /// </summary>
    public record ToDoList
    {
        /// <summary>
        /// Create a new <see cref="ToDoList"/>.
        /// </summary>
        /// <param name="minorFaction">
        /// The minor faction that the list focuses on.
        /// </param>
        public ToDoList(string minorFaction)
        {
            MinorFaction = minorFaction;
            Suggestions = new HashSet<Suggestion>();
        }

        /// <summary>
        /// The minor faction that the list focuses on.
        /// </summary>
        public string MinorFaction { get; }

        /// <summary>
        /// Suggestions.
        /// </summary>
        public ISet<Suggestion> Suggestions { get; }
    }
}
