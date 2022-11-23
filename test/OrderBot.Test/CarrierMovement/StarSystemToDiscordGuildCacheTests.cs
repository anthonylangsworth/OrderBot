using Microsoft.Extensions.Caching.Memory;
using NUnit.Framework;
using OrderBot.CarrierMovement;
using OrderBot.Core;
using OrderBot.EntityFramework;
using System.Transactions;

namespace OrderBot.Test.CarrierMovement;
internal class StarSystemToDiscordGuildCacheTests
{
    public OrderBotDbContextFactory DbContextFactory { get; set; } = null!;
    public OrderBotDbContext DbContext { get; set; } = null!;
    public IMemoryCache MemoryCache { get; set; } = null!;
    public StarSystemToDiscordGuildCache Cache { get; set; } = null!;
    public TransactionScope TransactionScope { get; set; } = null!;

    [SetUp]
    public void SetUp()
    {
        DbContextFactory = new();
        DbContext = DbContextFactory.CreateDbContext();
        MemoryCache = new MemoryCache(new MemoryCacheOptions());
        Cache = new(MemoryCache);
        TransactionScope = new();
    }

    [TearDown]
    public void TearDown()
    {
        TransactionScope.Dispose();
        DbContext.Dispose();
        MemoryCache.Dispose();
        DbContextFactory.Dispose();
    }

    [Test]
    [Ignore("Incomplete")]
    [TestCase("Sol", ExpectedResult = true)]
    [TestCase("Wolf 359", ExpectedResult = true)]
    [TestCase("Proxima Centauri", ExpectedResult = false)]
    public bool IsMonitoredStarSystem(string starSystemName)
    {
        DbContext.StarSystems.Add(new StarSystem() { Name = "Sol" });
        DbContext.StarSystems.Add(new StarSystem() { Name = "Wolf 359" });
        DbContext.SaveChanges();

        return Cache.IsMonitoredStarSystem(DbContext, starSystemName);
    }

    //[Test]
    //[TestCaseSource(nameof(GetGuildsForStarSystem_Source))]
    //public IReadOnlySet<string> GetGuildsForStarSystem(string starSystemName)
    //{
    //    Carrier priorityZero = new() { Name = CarrierNames.PriorityZero };
    //    Carrier invincible = new() { Name = CarrierNames.Invincible };
    //    Carrier myOtherShipIsAThargoid = new() { Name = CarrierNames.MyOtherShipIsAThargoid };
    //    DbContext.Carriers.AddRange(priorityZero, invincible, myOtherShipIsAThargoid);
    //    DbContext.SaveChanges();

    //    DiscordGuild discordGuild1 = new() { GuildId = 1 };
    //    DiscordGuild discordGuild2 = new() { GuildId = 2 };
    //    DiscordGuild discordGuild3 = new() { GuildId = 3 };
    //    DbContext.DiscordGuilds.AddRange(discordGuild1, discordGuild2, discordGuild3);
    //    DbContext.SaveChanges();

    //    discordGuild1.IgnoredCarriers.Add(priorityZero);
    //    discordGuild1.IgnoredCarriers.Add(invincible);
    //    discordGuild2.IgnoredCarriers.Add(myOtherShipIsAThargoid);
    //    discordGuild2.IgnoredCarriers.Add(invincible);
    //    DbContext.SaveChanges();

    //    return Cache.GetGuildsForStarSystem(DbContext, starSystemName);
    //}

    //public static IEnumerable<TestCaseData> GetGuildsForStarSystem_Source()
    //{
    //    Presence darkWheelInSol = new()
    //    {
    //        MinorFaction = new() { Name = "Dark Wheel" },
    //        StarSystem = new() { Name = "Sol" },
    //        Influence = 0.1
    //    };
    //    dbContext.Presences.Add(darkWheelInSol);
    //    DiscordGuild firstGuild = new() { Name = "First", GuildId = 20982408923432, CarrierMovementChannel = 9875264291 };
    //    firstGuild.SupportedMinorFactions.Add(darkWheelInSol.MinorFaction);
    //    dbContext.DiscordGuilds.Add(firstGuild);

    //    DiscordGuildPresenceGoal maintainDarkWheelInAlphaCentauri = new()
    //    {
    //        DiscordGuild = new() { Name = "Second", GuildId = 982340923874 },
    //        Presence = new()
    //        {
    //            MinorFaction = new() { Name = "Hutton Truckers" },
    //            StarSystem = darkWheelInSol.StarSystem,
    //            Influence = 0.2
    //        },
    //        Goal = MaintainGoal.Instance.Name
    //    };
    //    dbContext.DiscordGuildPresenceGoals.Add(maintainDarkWheelInAlphaCentauri);

    //    dbContext.SaveChanges();

    //    return new Dictionary<string, IDictionary<int, ulong?>>()
    //    {
    //        {
    //            darkWheelInSol.StarSystem.Name,
    //            new Dictionary<int, ulong?>()
    //            {
    //                {
    //                    firstGuild.Id,
    //                    firstGuild.CarrierMovementChannel
    //                },
    //                {
    //                    maintainDarkWheelInAlphaCentauri.DiscordGuild.Id,
    //                    maintainDarkWheelInAlphaCentauri.DiscordGuild.CarrierMovementChannel
    //                }
    //            }
    //        }
    //    };
    //}
}
