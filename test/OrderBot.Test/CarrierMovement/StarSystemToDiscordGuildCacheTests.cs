using NUnit.Framework;
using OrderBot.CarrierMovement;
using OrderBot.Core;
using OrderBot.Test.Samples;
using OrderBot.ToDo;

namespace OrderBot.Test.CarrierMovement;
internal class StarSystemToDiscordGuildCacheTests : CacheTest<StarSystemToDiscordGuildCache>
{
    public StarSystemToDiscordGuildCacheTests()
        : base(memoryCache => new StarSystemToDiscordGuildCache(memoryCache))
    {
        // Do nothing
    }

    [Test]
    [TestCase(StarSystemNames.Archernar, ExpectedResult = false, Description = "Unknown system")]
    [TestCase(StarSystemNames.Caelano, ExpectedResult = false, Description = "Unknown system")]
    public bool IsMonitoredStarSystem_None(string starSystemName)
    {
        return Cache.IsMonitoredStarSystem(DbContext, starSystemName);
    }

    [Test]
    [TestCase(StarSystemNames.Sol, ExpectedResult = true, Description = "Supported minor faction")]
    [TestCase(StarSystemNames.Wolf359, ExpectedResult = true, Description = "Two supported minor factions")]
    [TestCase(StarSystemNames.AlphaCentauri, ExpectedResult = true, Description = "Minor faction and goal")]
    [TestCase(StarSystemNames.BarnardsStar, ExpectedResult = true, Description = "One goal only")]
    [TestCase(StarSystemNames.Archernar, ExpectedResult = false, Description = "Known system but no goals or presences")]
    [TestCase(StarSystemNames.Caelano, ExpectedResult = false, Description = "Unknown system")]
    public bool IsMonitoredStarSystem(string starSystemName)
    {
        StarSystem sol = new() { Name = StarSystemNames.Sol };
        StarSystem wolf359 = new() { Name = StarSystemNames.Wolf359 };
        StarSystem alphaCentauri = new() { Name = StarSystemNames.AlphaCentauri };
        StarSystem barnardsStar = new() { Name = StarSystemNames.BarnardsStar };
        StarSystem archenar = new() { Name = StarSystemNames.Archernar };
        DbContext.StarSystems.AddRange(sol, wolf359, alphaCentauri, barnardsStar, archenar);

        MinorFaction darkWheel = new() { Name = MinorFactionNames.DarkWheel };
        MinorFaction eurybiaBlueMafia = new() { Name = MinorFactionNames.EurybiaBlueMafia };
        MinorFaction azimuthBiotech = new() { Name = MinorFactionNames.AzimuthBiotech };
        DbContext.MinorFactions.AddRange(darkWheel, eurybiaBlueMafia, azimuthBiotech);

        DiscordGuild discordGuid1 = new() { GuildId = 123455667 };
        DiscordGuild discordGuid2 = new() { GuildId = 936593640 };
        DbContext.DiscordGuilds.AddRange(discordGuid1, discordGuid2);

        DbContext.SaveChanges();

        discordGuid1.SupportedMinorFactions.Add(darkWheel);
        DbContext.Presences.Add(new Presence() { MinorFaction = darkWheel, StarSystem = sol });
        DbContext.Presences.Add(new Presence() { MinorFaction = eurybiaBlueMafia, StarSystem = wolf359 });
        DbContext.Presences.Add(new Presence() { MinorFaction = darkWheel, StarSystem = wolf359 });
        DbContext.SaveChanges();

        DbContext.DiscordGuildPresenceGoals.Add(new()
        {
            DiscordGuild = discordGuid2,
            Presence = new()
            {
                MinorFaction = azimuthBiotech,
                StarSystem = alphaCentauri
            },
            Goal = ExpandGoal.Instance.Name
        });
        DbContext.DiscordGuildPresenceGoals.Add(new()
        {
            DiscordGuild = discordGuid2,
            Presence = new()
            {
                MinorFaction = azimuthBiotech,
                StarSystem = barnardsStar
            },
            Goal = RetreatGoal.Instance.Name
        });
        DbContext.SaveChanges();

        return Cache.IsMonitoredStarSystem(DbContext, starSystemName);
    }
}
