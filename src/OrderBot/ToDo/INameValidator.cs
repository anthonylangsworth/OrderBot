namespace OrderBot.ToDo;

/// <summary>
/// Validate minor factions and star systems for <see cref="ToDoListApi"/>.
/// </summary>
public interface INameValidator
{
    /// <summary>
    /// Is <paramref name="minorFactionName"/> a valid minor faction?
    /// </summary>
    /// <param name="minorFactionName">
    /// The name to test.
    /// </param>
    /// <returns>
    /// <c>true</c> if it is known, <c>false</c> otherwise.
    /// </returns>
    Task<bool> IsKnownMinorFaction(string minorFactionName);

    /// <summary>
    /// Is <paramref name="starSystemName"/> a valid star system?
    /// </summary>
    /// <param name="starSystemName">
    /// The name to test.
    /// </param>
    /// <returns>
    /// <c>true</c> if it is known, <c>false</c> otherwise.
    /// </returns>
    Task<bool> IsKnownStarSystem(string starSystemName);
}
