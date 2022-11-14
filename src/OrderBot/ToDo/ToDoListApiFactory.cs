using Discord;
using OrderBot.EntityFramework;

namespace OrderBot.ToDo;

/// <summary>
/// Abstract the creation of a <see cref="ToDoListApi"/>.
/// </summary>
public class ToDoListApiFactory
{
    /// <summary>
    /// Create a <see cref="ToDoListApi"/>
    /// </summary>
    /// <param name="dbContext">
    /// The <see cref="OrderBotDbContext"/> to use.
    /// </param>
    /// <param name="guild">
    /// The <see cref="IGuild"/> to act on or for.
    /// </param>
    /// <returns>
    /// A <see cref="ToDoListApi"/>.
    /// </returns>
    public ToDoListApi CreateApi(OrderBotDbContext dbContext, IGuild guild)
    {
        return new(dbContext, guild);
    }
}
