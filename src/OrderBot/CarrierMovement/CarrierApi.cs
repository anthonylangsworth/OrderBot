using Discord;
using Microsoft.EntityFrameworkCore;
using OrderBot.Core;
using OrderBot.Discord;
using OrderBot.EntityFramework;
using OrderBot.ToDo;

namespace OrderBot.CarrierMovement;
public class CarrierApi
{
    /// <summary>
    /// Create a new <see cref="ToDoListApi"/>.
    /// </summary>
    /// <param name="dbContext">
    /// The <see cref="OrderBotDbContext"/> to use.
    /// </param>
    /// <param name="guild">
    /// The <see cref="IGuild"/> to act on or for.
    /// </param>
    /// <param name="validator">
    /// Used to validate minor factions and star systems via web services.
    /// </param>
    public CarrierApi(OrderBotDbContext dbContext, IGuild guild)
    {
        DbContext = dbContext;
        Guild = guild;
    }

    public OrderBotDbContext DbContext { get; }
    public IGuild Guild { get; }

    public void AddIgnoredCarriers(IEnumerable<string> names)
    {
        DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(DbContext, Guild,
            DbContext.DiscordGuilds.Include(dg => dg.IgnoredCarriers));
        foreach (string name in names)
        {
            string serialNumber = Carrier.GetSerialNumber(name);
            Carrier? ignoredCarrier = discordGuild.IgnoredCarriers.FirstOrDefault(c => c.SerialNumber == serialNumber);
            if (!discordGuild.IgnoredCarriers.Any(c => c.SerialNumber == serialNumber))
            {
                Carrier? carrier = DbContext.Carriers.FirstOrDefault(c => c.SerialNumber == serialNumber);
                if (carrier == null)
                {
                    carrier = new Carrier() { Name = name };
                    DbContext.Carriers.Add(carrier);
                }
                discordGuild.IgnoredCarriers.Add(carrier);
            }
            DbContext.SaveChanges();
        }
    }

    public IEnumerable<Carrier> ListIgnoredCarriers()
    {
        DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(DbContext, Guild,
            DbContext.DiscordGuilds.Include(dg => dg.IgnoredCarriers));
        return discordGuild.IgnoredCarriers.OrderBy(c => c.Name);
    }

    public void RemoveIgnoredCarrier(string name)
    {
        string serialNumber = Carrier.GetSerialNumber(name);
        DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(DbContext, Guild,
            DbContext.DiscordGuilds.Include(dg => dg.IgnoredCarriers));
        Carrier? ignoredCarrier = discordGuild.IgnoredCarriers.FirstOrDefault(c => c.SerialNumber == serialNumber);
        if (ignoredCarrier != null)
        {
            discordGuild.IgnoredCarriers.Remove(ignoredCarrier);
        }
        DbContext.SaveChanges();
    }
}
