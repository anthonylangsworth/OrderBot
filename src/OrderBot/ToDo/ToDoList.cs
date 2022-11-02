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
            Pro = new List<InfluenceSuggestion>();
            Anti = new List<InfluenceSuggestion>();
            ProSecurity = new List<SecuritySuggestion>();
            Wars = new List<ConflictSuggestion>();
            Elections = new List<ConflictSuggestion>();
        }

        /// <summary>
        /// The minor faction that the list focuses on.
        /// </summary>
        public string MinorFaction { get; }

        /// <summary>
        /// Work for the specified minor faction.
        /// </summary>
        public IList<InfluenceSuggestion> Pro { get; }

        /// <summary>
        /// Work against the specified minor faction.
        /// </summary>
        public IList<InfluenceSuggestion> Anti { get; }

        /// <summary>
        /// Work for the specified minor faction.
        /// </summary>
        public IList<SecuritySuggestion> ProSecurity { get; }

        /// <summary>
        /// Fight for the specified minor faction.
        /// </summary>
        public IList<ConflictSuggestion> Wars { get; }

        /// <summary>
        /// Fight for the specified minor faction.
        /// </summary>
        public IList<ConflictSuggestion> Elections { get; }
    }
}
