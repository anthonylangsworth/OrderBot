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
    public ToDoListApiFactory(OrderBotDbContext dbContext)
    {
        DbContext = dbContext;
    }

    /// <summary>
    /// The <see cref="OrderBotDbContext"/> to use.
    /// </summary>
    internal OrderBotDbContext DbContext { get; }

    /// <summary>
    /// Create a <see cref="ToDoListApi"/>
    /// </summary>
    /// <param name="guild">
    /// The <see cref="IGuild"/> to act on or for.
    /// </param>
    /// <returns>
    /// A <see cref="ToDoListApi"/>.
    /// </returns>
    public ToDoListApi CreateApi(IGuild guild)
    {
        return new(DbContext, guild);
    }
}
