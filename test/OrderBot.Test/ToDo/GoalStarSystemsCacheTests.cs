using NUnit.Framework;
using OrderBot.CarrierMovement;
using OrderBot.Core;
using OrderBot.Test.Samples;
using OrderBot.ToDo;

namespace OrderBot.Test.ToDo;
internal class GoalStarSystemsCacheTests : CacheTest<GoalStarSystemsCache>
{
    public GoalStarSystemsCacheTests()
        : base(memoryCache => new GoalStarSystemsCache(memoryCache))
    {
        // Do nothing
    }

    [Test]
    [TestCase(StarSystemNames.Betelgeuse, ExpectedResult = false)]
    [TestCase(StarSystemNames.Sirius, ExpectedResult = false)]
    public bool HasGoal_None(string starSystemName)
    {
        return Cache.HasGoal(DbContext, starSystemName);
    }

    [Test]
    [TestCase(StarSystemNames.Sirius, ExpectedResult = true)]
    [TestCase(StarSystemNames.Betelgeuse, ExpectedResult = true)]
    [TestCase(StarSystemNames.Celaeno, ExpectedResult = false)]
    [TestCase(StarSystemNames.Asterope, ExpectedResult = false)]
    public bool HasGoal(string starSystemName)
    {
        StarSystem sirius = new() { Name = StarSystemNames.Sirius };
        StarSystem betelgeuse = new() { Name = StarSystemNames.Betelgeuse };
        StarSystem caelano = new() { Name = StarSystemNames.Celaeno };
        DbContext.StarSystems.AddRange(sirius, betelgeuse, caelano);

        MinorFaction canonn = new() { Name = MinorFactionNames.Canonn };
        MinorFaction huttonTruckers = new() { Name = MinorFactionNames.HuttonTruckers };
        MinorFaction azimuthBiotech = new() { Name = MinorFactionNames.AzimuthBiotech };
        DbContext.MinorFactions.AddRange(canonn, huttonTruckers, azimuthBiotech);

        DiscordGuild discordGuild1 = new() { GuildId = 1 };
        DiscordGuild discordGuild2 = new() { GuildId = 2 };
        DiscordGuild discordGuild3 = new() { GuildId = 3 };
        DbContext.DiscordGuilds.AddRange(discordGuild1, discordGuild2, discordGuild3);

        DbContext.SaveChanges();

        DbContext.DiscordGuildPresenceGoals.Add(new DiscordGuildPresenceGoal()
        {
            DiscordGuild = discordGuild1,
            Presence = new Presence()
            {
                StarSystem = sirius,
                MinorFaction = canonn
            },
            Goal = MaintainGoal.Instance.Name
        });
        DbContext.DiscordGuildPresenceGoals.Add(new DiscordGuildPresenceGoal()
        {
            DiscordGuild = discordGuild1,
            Presence = new Presence()
            {
                StarSystem = betelgeuse,
                MinorFaction = huttonTruckers
            },
            Goal = RetreatGoal.Instance.Name
        });
        DbContext.DiscordGuildPresenceGoals.Add(new DiscordGuildPresenceGoal()
        {
            DiscordGuild = discordGuild2,
            Presence = new Presence()
            {
                StarSystem = sirius,
                MinorFaction = azimuthBiotech
            },
            Goal = ControlGoal.Instance.Name
        });
        DbContext.SaveChanges();

        return Cache.HasGoal(DbContext, starSystemName);
    }
}
