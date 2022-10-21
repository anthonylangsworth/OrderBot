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
            Pro = new List<InfluenceInitiatedAction>();
            Anti = new List<InfluenceInitiatedAction>();
        }

        /// <summary>
        /// The minor faction that the list focuses on.
        /// </summary>
        public string MinorFaction { get; }

        /// <summary>
        /// Work for the specified minor faction.
        /// </summary>
        public IList<InfluenceInitiatedAction> Pro { get; }

        /// <summary>
        /// Work against the specified minor faction.
        /// </summary>
        public IList<InfluenceInitiatedAction> Anti { get; }
    }
}
