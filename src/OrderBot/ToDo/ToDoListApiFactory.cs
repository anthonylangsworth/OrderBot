using Discord;
using OrderBot.EntityFramework;

namespace OrderBot.ToDo;

/// <summary>
/// Abstract the creation of a <see cref="ToDoListApi"/>.
/// </summary>
public class ToDoListApiFactory
{
    /// <summary>
    /// Create a new <see cref="ToDoListApiFactory"/>.
    /// </summary>
    /// <param name="dbContext">
    /// The <see cref="OrderBotDbContext"/> to use.
    /// </param>
    /// <param name="validator">
    /// Used to validate minor factions and star systems.
    /// </param>
    public ToDoListApiFactory(OrderBotDbContext dbContext, INameValidator validator)
    {
        DbContext = dbContext;
        Validator = validator;
    }

    /// <summary>
    /// The <see cref="OrderBotDbContext"/> to use.
    /// </summary>
    internal OrderBotDbContext DbContext { get; }
    /// <summary>
    /// Used to validate minor factions and star systems.
    /// </summary>
    public INameValidator Validator { get; }

    /// <summary>
    /// Create a <see cref="ToDoListApi"/>
    /// </summary>
    /// <returns>
    /// A <see cref="ToDoListApi"/>.
    /// </returns>
    public ToDoListApi CreateApi()
    {
        return new(DbContext, Validator);
    }
}
