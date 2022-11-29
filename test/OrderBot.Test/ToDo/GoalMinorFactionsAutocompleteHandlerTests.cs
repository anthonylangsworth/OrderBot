using NUnit.Framework;
using OrderBot.Core;
using OrderBot.Test.Samples;
using OrderBot.ToDo;

namespace OrderBot.Test.ToDo;

internal class GoalMinorFactionsAutocompleteHandlerTests : DbTest
{
    [Test]
    [TestCase("", 1UL, new string[] { MinorFactionNames.AXI, MinorFactionNames.AzimuthBiotech })]
    [TestCase("a", 1UL, new string[] { MinorFactionNames.AXI, MinorFactionNames.AzimuthBiotech })]
    [TestCase("A", 1UL, new string[] { MinorFactionNames.AXI, MinorFactionNames.AzimuthBiotech })]
    [TestCase("An", 1UL, new string[] { MinorFactionNames.AXI })]
    [TestCase("e", 1UL, new string[0])]
    [TestCase("c", 1UL, new string[0])]
    [TestCase("", 2UL, new string[] { MinorFactionNames.EurybiaBlueMafia })]
    [TestCase("e", 2UL, new string[] { MinorFactionNames.EurybiaBlueMafia })]
    [TestCase("eu", 2UL, new string[] { MinorFactionNames.EurybiaBlueMafia })]
    [TestCase("eur", 2UL, new string[] { MinorFactionNames.EurybiaBlueMafia })]
    [TestCase("a", 2UL, new string[0])]
    [TestCase("c", 2UL, new string[0])]
    public void GetMinorFactions(string enteredValue, ulong discordGuildId, string[] expectedResults)
    {
        GoalMinorFactionsAutocompleteHandler handler = new(DbContext);

        DiscordGuild discordGuild1 = new() { GuildId = 1 };
        DiscordGuild discordGuild2 = new() { GuildId = 2 };
        DbContext.DiscordGuilds.AddRange(discordGuild2);

        MinorFaction eurybiaBlueMafia = new() { Name = MinorFactionNames.EurybiaBlueMafia };
        MinorFaction azimuthBiotech = new() { Name = MinorFactionNames.AzimuthBiotech };
        MinorFaction axi = new() { Name = MinorFactionNames.AXI };
        MinorFaction cannon = new() { Name = MinorFactionNames.Canonn }; // Not used in any goals
        DbContext.MinorFactions.AddRange(eurybiaBlueMafia, azimuthBiotech, axi, cannon);

        StarSystem maia = new() { Name = StarSystemNames.Maia };
        StarSystem asterope = new() { Name = StarSystemNames.Asterope };
        DbContext.StarSystems.AddRange(maia, asterope);

        DbContext.SaveChanges();

        DbContext.DiscordGuildPresenceGoals.Add(new()
        {
            DiscordGuild = discordGuild1,
            Presence = new()
            {
                MinorFaction = axi,
                StarSystem = asterope
            },
            Goal = MaintainGoal.Instance.Name
        });
        DbContext.DiscordGuildPresenceGoals.Add(new()
        {
            DiscordGuild = discordGuild1,
            Presence = new()
            {
                MinorFaction = azimuthBiotech,
                StarSystem = asterope
            },
            Goal = MaintainGoal.Instance.Name
        });
        DbContext.DiscordGuildPresenceGoals.Add(new()
        {
            DiscordGuild = discordGuild1,
            Presence = new()
            {
                MinorFaction = azimuthBiotech,
                StarSystem = maia
            },
            Goal = MaintainGoal.Instance.Name
        });
        DbContext.DiscordGuildPresenceGoals.Add(new()
        {
            DiscordGuild = discordGuild2,
            Presence = new()
            {
                MinorFaction = eurybiaBlueMafia,
                StarSystem = maia
            },
            Goal = MaintainGoal.Instance.Name
        });
        DbContext.SaveChanges();

        Assert.That(handler.GetMinorFactions(discordGuildId, enteredValue),
            Is.EqualTo(expectedResults));
    }
}
