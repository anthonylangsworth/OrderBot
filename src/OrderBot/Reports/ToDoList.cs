namespace OrderBot.Reports
{
    /// <summary>
    /// A To Do List, containing the various work for commanders to support a minor faction.
    /// </summary>
    internal class ToDoList
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
            Pro = new List<InfluenceAction>();
            Anti = new List<InfluenceAction>();
        }

        /// <summary>
        /// The minor faction that the list focuses on.
        /// </summary>
        public string MinorFaction { get; }

        /// <summary>
        /// Work for the specified minor faction.
        /// </summary>
        public IList<InfluenceAction> Pro { get; }

        /// <summary>
        /// Work against the specified minor faction.
        /// </summary>
        public IList<InfluenceAction> Anti { get; }
    }
}
