using NUnit.Framework;
using OrderBot.Core;
using OrderBot.Test.Samples;
using OrderBot.ToDo;

namespace OrderBot.Test.ToDo;
internal class SupportedMinorFactionsCacheTests : CacheTest<SupportedMinorFactionsCache>
{
    public SupportedMinorFactionsCacheTests()
        : base(memoryCache => new SupportedMinorFactionsCache(memoryCache))
    {
        // Do nothing
    }

    [Test]
    [TestCase(MinorFactionNames.AzimuthBiotech, ExpectedResult = false)]
    [TestCase(MinorFactionNames.Canonn, ExpectedResult = false)]
    public bool HasGoal_None(string minorFactionName)
    {
        return Cache.IsSupported(DbContext, minorFactionName);
    }

    [Test]
    [TestCase(MinorFactionNames.Canonn, ExpectedResult = true)]
    [TestCase(MinorFactionNames.HuttonTruckers, ExpectedResult = true)]
    [TestCase(MinorFactionNames.AzimuthBiotech, ExpectedResult = false)]
    [TestCase(MinorFactionNames.EurybiaBlueMafia, ExpectedResult = false)]
    public bool HasGoal(string minorFactionName)
    {
        MinorFaction canonn = new() { Name = MinorFactionNames.Canonn };
        MinorFaction huttonTruckers = new() { Name = MinorFactionNames.HuttonTruckers };
        MinorFaction azimuthBiotech = new() { Name = MinorFactionNames.AzimuthBiotech };
        DbContext.MinorFactions.AddRange(canonn, huttonTruckers, azimuthBiotech);

        DiscordGuild discordGuild1 = new() { GuildId = 1 };
        DiscordGuild discordGuild2 = new() { GuildId = 2 };
        DiscordGuild discordGuild3 = new() { GuildId = 3 };

        discordGuild1.SupportedMinorFactions.Add(canonn);
        discordGuild1.SupportedMinorFactions.Add(huttonTruckers);
        discordGuild2.SupportedMinorFactions.Add(canonn);
        DbContext.DiscordGuilds.AddRange(discordGuild1, discordGuild2, discordGuild3);
        DbContext.SaveChanges();

        return Cache.IsSupported(DbContext, minorFactionName);
    }

}
