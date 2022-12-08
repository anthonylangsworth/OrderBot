using Discord;
using OrderBot.EntityFramework;
using OrderBot.ToDo;

namespace OrderBot.CarrierMovement;
public class CarrierApiFactory
{
    /// <summary>
    /// Create a new <see cref="ToDoListApiFactory"/>.
    /// </summary>
    /// <param name="dbContext">
    /// The <see cref="OrderBotDbContext"/> to use.
    /// </param>
    public CarrierApiFactory(OrderBotDbContext dbContext)
    {
        DbContext = dbContext;
    }

    /// <summary>
    /// The <see cref="OrderBotDbContext"/> to use.
    /// </summary>
    private OrderBotDbContext DbContext { get; }

    /// <summary>
    /// Create a <see cref="CarrierApi"/>
    /// </summary>
    /// <param name="guild">
    /// The <see cref="IGuild"/> to act on or for.
    /// </param>
    /// <returns>
    /// A <see cref="ToDoListApi"/>.
    /// </returns>
    public CarrierApi CreateApi(IGuild guild)
    {
        return new(DbContext, guild);
    }
}
