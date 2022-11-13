using System.Runtime.Serialization;

namespace OrderBot.ToDo;

/// <summary>
/// A <see cref="ToDoList"/> is generated with an unknown goal.
/// </summary>
[Serializable]
internal class UnknownGoalException : Exception
{
    /// <summary>
    /// Create a new <see cref="UnknownGoalException"/>.
    /// </summary>
    /// <param name="goalName"></param>
    /// <param name="starSystemName"></param>
    /// <param name="minorFactionName"></param>
    public UnknownGoalException(string goalName, string starSystemName, string minorFactionName)
        : base($"Unknown goal '{goalName}' for star system '{starSystemName}' for minor faction '{minorFactionName}'")
    {
        Goal = goalName;
        StarSystem = starSystemName;
        MinorFaction = minorFactionName;
    }

    protected UnknownGoalException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        throw new NotImplementedException();
    }

    public string Goal { get; }
    public string StarSystem { get; }
    public string MinorFaction { get; }
}
