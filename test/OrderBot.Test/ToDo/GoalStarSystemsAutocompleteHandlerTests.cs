using NUnit.Framework;
using OrderBot.Core;
using OrderBot.Test.Samples;
using OrderBot.ToDo;

namespace OrderBot.Test.ToDo;

internal class GoalStarSystemsAutocompleteHandlerTests : DbTest
{
    [Test]
    [TestCase("", 1UL, new string[] { StarSystemNames.Asterope, StarSystemNames.Atlas })]
    [TestCase("a", 1UL, new string[] { StarSystemNames.Asterope, StarSystemNames.Atlas })]
    [TestCase("A", 1UL, new string[] { StarSystemNames.Asterope, StarSystemNames.Atlas })]
    [TestCase("As", 1UL, new string[] { StarSystemNames.Asterope })]
    [TestCase("At", 1UL, new string[] { StarSystemNames.Atlas })]
    [TestCase("Atl", 1UL, new string[] { StarSystemNames.Atlas })]
    [TestCase("m", 1UL, new string[0])]
    [TestCase("", 2UL, new string[] { StarSystemNames.Maia })]
    [TestCase("m", 2UL, new string[] { StarSystemNames.Maia })]
    [TestCase("ma", 2UL, new string[] { StarSystemNames.Maia })]
    [TestCase("a", 2UL, new string[0])]
    public void GetStarSystems(string enteredValue, ulong discordGuildId, string[] expectedResults)
    {
        GoalStarSystemsAutocompleteHandler handler = new(DbContext);

        DiscordGuild discordGuild1 = new() { GuildId = 1 };
        DiscordGuild discordGuild2 = new() { GuildId = 2 };
        DbContext.DiscordGuilds.AddRange(discordGuild2);

        MinorFaction eurybiaBlueMafia = new() { Name = MinorFactionNames.EurybiaBlueMafia };
        MinorFaction azimuthBiotech = new() { Name = MinorFactionNames.AzimuthBiotech };
        MinorFaction axi = new() { Name = MinorFactionNames.AXI };
        DbContext.MinorFactions.AddRange(eurybiaBlueMafia, azimuthBiotech, axi);

        StarSystem maia = new() { Name = StarSystemNames.Maia };
        StarSystem asterope = new() { Name = StarSystemNames.Asterope };
        StarSystem atlas = new() { Name = StarSystemNames.Atlas };
        StarSystem alphaCentauri = new() { Name = StarSystemNames.AlphaCentauri }; // Not used in a goal
        DbContext.StarSystems.AddRange(maia, asterope, atlas, alphaCentauri);

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
                MinorFaction = axi,
                StarSystem = atlas
            },
            Goal = MaintainGoal.Instance.Name
        });
        DbContext.DiscordGuildPresenceGoals.Add(new()
        {
            DiscordGuild = discordGuild1,
            Presence = new()
            {
                MinorFaction = azimuthBiotech,
                StarSystem = atlas
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

        Assert.That(handler.GetStarSystems(discordGuildId, enteredValue),
            Is.EqualTo(expectedResults));
    }
}
