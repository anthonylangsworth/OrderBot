namespace OrderBot.ToDo;

/// <summary>
/// Minor factions used to filter messages processed by <see cref="ToDoListMessageProcessor"/>.
/// </summary>
internal abstract class MinorFactionNameFilter
{
    /// <summary>
    /// Does the given name match?
    /// </summary>
    /// <param name="name">
    /// The name to match.
    /// </param>
    /// <returns>
    /// Return <see cref="true"/> if the name matches, <see cref="false"/> otherwise.
    /// </returns>
    public abstract bool Matches(string name);
}
