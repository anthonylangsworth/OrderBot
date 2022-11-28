using NUnit.Framework;
using OrderBot.Core;
using OrderBot.ToDo;

namespace OrderBot.Test.ToDo;

internal class ToDoListGeneratorTests : DbTest
{
    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        DiscordGuild = new DiscordGuild() { GuildId = 382284915670253569, Name = "Test Guild" };
        PurplePeopleEaters = new MinorFaction() { Name = "Purple People Eaters" };
        DbContext.MinorFactions.Add(PurplePeopleEaters);
        DbContext.DiscordGuilds.Add(DiscordGuild);
        DbContext.SaveChanges();
        DiscordGuild.SupportedMinorFactions.Add(PurplePeopleEaters);
        DbContext.SaveChanges();
    }

    internal DiscordGuild DiscordGuild { get; set; } = null!;
    internal MinorFaction PurplePeopleEaters { get; set; } = null!;

    [Test]
    public void Ctor()
    {
        ToDoListGenerator generator = new(DbContext);
        Assert.That(generator.DbContext, Is.EqualTo(DbContext));
    }

    [Test]
    public void Generate_Empty()
    {
        ToDoListGenerator generator = new(DbContext);
        ToDoList toDoList = generator.Generate(DiscordGuild.GuildId);
        Assert.That(toDoList.Suggestions, Is.Empty);
    }

    [Test]
    public void Generate_SingleSystem_DefaultGoal_None()
    {
        StarSystem alphCentauri = new() { Name = "Alpha Centauri", LastUpdated = DateTime.UtcNow };
        Presence starSystemMinorFaction =
            new()
            {
                MinorFaction = PurplePeopleEaters,
                StarSystem = alphCentauri,
                Influence = (ControlGoal.LowerInfluenceThreshold + ControlGoal.UpperInfluenceThreshold) / 2
            };
        DiscordGuildPresenceGoal discordGuild = new()
        {
            DiscordGuild = DiscordGuild,
            Presence = starSystemMinorFaction,
            Goal = Goals.Default.Name
        };
        DbContext.DiscordGuildPresenceGoals.Add(discordGuild);
        DbContext.SaveChanges();

        ToDoListGenerator generator = new(DbContext);
        ToDoList toDoList = generator.Generate(DiscordGuild.GuildId);
        Assert.That(toDoList.MinorFaction, Is.EqualTo(PurplePeopleEaters.Name));
        Assert.That(toDoList.Suggestions, Is.Empty);
    }

    [Test]
    public void Generate_SingleSystem_DefaultGoal_Pro()
    {
        StarSystem alphCentauri = new() { Name = "Alpha Centauri", LastUpdated = DateTime.UtcNow };
        Presence starSystemMinorFaction =
            new() { MinorFaction = PurplePeopleEaters, StarSystem = alphCentauri, Influence = ControlGoal.LowerInfluenceThreshold - 0.01 };
        DiscordGuildPresenceGoal discordGuild = new()
        {
            DiscordGuild = DiscordGuild,
            Presence = starSystemMinorFaction,
            Goal = Goals.Default.Name
        };
        DbContext.DiscordGuildPresenceGoals.Add(discordGuild);
        DbContext.SaveChanges();

        ToDoListGenerator generator = new(DbContext);
        ToDoList toDoList = generator.Generate(DiscordGuild.GuildId);
        Assert.That(toDoList.MinorFaction, Is.EqualTo(PurplePeopleEaters.Name));
        Assert.That(toDoList.Suggestions,
            Is.EquivalentTo(new[] { new InfluenceSuggestion(alphCentauri, PurplePeopleEaters, true, starSystemMinorFaction.Influence) })
              .Using(DbInfluenceSuggestionEqualityComparer.Instance));
    }

    [Test]
    public void Generate_SingleSystem_DefaultGoal_Anti()
    {
        StarSystem alphCentauri = new() { Name = "Alpha Centauri", LastUpdated = DateTime.UtcNow };
        Presence starSystemMinorFaction =
            new() { MinorFaction = PurplePeopleEaters, StarSystem = alphCentauri, Influence = ControlGoal.UpperInfluenceThreshold + 0.01 };
        DiscordGuildPresenceGoal discordGuild = new()
        {
            DiscordGuild = DiscordGuild,
            Presence = starSystemMinorFaction,
            Goal = Goals.Default.Name
        };
        DbContext.DiscordGuildPresenceGoals.Add(discordGuild);
        DbContext.SaveChanges();

        ToDoListGenerator generator = new(DbContext);
        ToDoList toDoList = generator.Generate(DiscordGuild.GuildId);
        Assert.That(toDoList.MinorFaction, Is.EqualTo(PurplePeopleEaters.Name));
        Assert.That(toDoList.Suggestions,
            Is.EquivalentTo(new[] { new InfluenceSuggestion(alphCentauri, PurplePeopleEaters, true, starSystemMinorFaction.Influence) })
              .Using(DbInfluenceSuggestionEqualityComparer.Instance));
    }

    [Test]
    public void Generate_SingleSystem_DefaultGoal_Unrelated()
    {
        string differentMinorFactionName = "Star Gazers";
        StarSystem alphCentauri = new() { Name = "Alpha Centauri", LastUpdated = DateTime.UtcNow };
        MinorFaction purplePeopleEaters = new() { Name = differentMinorFactionName };
        Presence starSystemMinorFaction =
            new() { MinorFaction = purplePeopleEaters, StarSystem = alphCentauri, Influence = 0 };
        DiscordGuildPresenceGoal discordGuild = new()
        {
            DiscordGuild = DiscordGuild,
            Presence = starSystemMinorFaction,
            Goal = Goals.Default.Name
        };
        DbContext.DiscordGuildPresenceGoals.Add(discordGuild);
        DbContext.SaveChanges();

        ToDoListGenerator generator = new(DbContext);
        ToDoList toDoList = generator.Generate(DiscordGuild.GuildId);
        Assert.That(toDoList.MinorFaction, Is.EqualTo(PurplePeopleEaters.Name));
        Assert.That(toDoList.Suggestions,
            Is.EquivalentTo(new[] { new InfluenceSuggestion(alphCentauri, PurplePeopleEaters, true, starSystemMinorFaction.Influence) })
              .Using(DbInfluenceSuggestionEqualityComparer.Instance));
    }

    [Test]
    public void Generate_MultipleSystems_DefaultGoal()
    {
        StarSystem alphCentauri = new() { Name = "Alpha Centauri", LastUpdated = DateTime.UtcNow };
        StarSystem maia = new() { Name = "Maia", LastUpdated = DateTime.UtcNow };
        DiscordGuildPresenceGoal purplePeopleEastersAlphaCentauri = new()
        {
            DiscordGuild = DiscordGuild,
            Presence = new() { MinorFaction = PurplePeopleEaters, StarSystem = alphCentauri, Influence = ControlGoal.LowerInfluenceThreshold - 0.01 },
            Goal = Goals.Default.Name
        };
        DbContext.DiscordGuildPresenceGoals.Add(purplePeopleEastersAlphaCentauri);
        DiscordGuildPresenceGoal purplePeopleEastersMaia = new()
        {
            DiscordGuild = DiscordGuild,
            Presence = new() { MinorFaction = PurplePeopleEaters, StarSystem = maia, Influence = ControlGoal.UpperInfluenceThreshold + 0.01 },
            Goal = Goals.Default.Name
        };
        DbContext.DiscordGuildPresenceGoals.Add(purplePeopleEastersMaia);
        DbContext.SaveChanges();

        ToDoListGenerator generator = new(DbContext);
        ToDoList toDoList = generator.Generate(DiscordGuild.GuildId);
        Assert.That(toDoList.MinorFaction, Is.EqualTo(PurplePeopleEaters.Name));
        Assert.That(toDoList.Suggestions, Is.EquivalentTo(
           new Suggestion[] {
               new InfluenceSuggestion(alphCentauri, purplePeopleEastersAlphaCentauri.Presence.MinorFaction, true, purplePeopleEastersAlphaCentauri.Presence.Influence),
               new InfluenceSuggestion(maia, purplePeopleEastersMaia.Presence.MinorFaction, false, purplePeopleEastersMaia.Presence.Influence)
           }).Using(DbInfluenceSuggestionEqualityComparer.Instance));
    }

    [Test]
    public void Generate_Conflict_NoGoal()
    {
        StarSystem celaeno = new() { Name = "Celaeno" };
        MinorFaction yellowSubmariners = new() { Name = "Yellow Submariners" };
        Presence purplePeopleEastersInCelaeno = new()
        {
            MinorFaction = PurplePeopleEaters,
            StarSystem = celaeno,
            Influence = 0.4
        };
        Presence yellowSubmarinersInCalaeno = new()
        {
            MinorFaction = yellowSubmariners,
            StarSystem = celaeno,
            Influence = 0.3
        };
        DbContext.Presences.AddRange(purplePeopleEastersInCelaeno, yellowSubmarinersInCalaeno);
        Conflict conflict = new()
        {
            MinorFaction1 = PurplePeopleEaters,
            MinorFaction1WonDays = 2,
            MinorFaction2 = yellowSubmariners,
            MinorFaction2WonDays = 1,
            StarSystem = celaeno,
            WarType = WarType.War,
            Status = ConflictStatus.Active
        };
        DbContext.Conflicts.Add(conflict);
        DbContext.SaveChanges();

        ToDoListGenerator generator = new(DbContext);
        ToDoList toDoList = generator.Generate(DiscordGuild.GuildId);
        Assert.That(toDoList.MinorFaction, Is.EqualTo(PurplePeopleEaters.Name));
        Assert.That(toDoList.Suggestions, Is.EquivalentTo(
           new Suggestion[] {
               new ConflictSuggestion(conflict.StarSystem, conflict.MinorFaction1, conflict.MinorFaction1WonDays,
                   conflict.MinorFaction2,conflict.MinorFaction2WonDays, ConflictState.CloseVictory, conflict.WarType)
           }).Using(DbConflictSuggestionEqualityComparer.Instance));
    }

    [Test]
    public void Generate_Conflict_Goal()
    {
        StarSystem celaeno = new() { Name = "Celaeno" };
        MinorFaction yellowSubmariners = new() { Name = "Yellow Submariners" };
        Presence purplePeopleEastersInCelaeno = new()
        {
            MinorFaction = PurplePeopleEaters,
            StarSystem = celaeno,
            Influence = 0.4
        };
        Presence yellowSubmarinersInCalaeno = new()
        {
            MinorFaction = yellowSubmariners,
            StarSystem = celaeno,
            Influence = 0.3
        };
        DbContext.Presences.AddRange(purplePeopleEastersInCelaeno, yellowSubmarinersInCalaeno);
        Conflict conflict = new()
        {
            MinorFaction1 = PurplePeopleEaters,
            MinorFaction1WonDays = 2,
            MinorFaction2 = yellowSubmariners,
            MinorFaction2WonDays = 1,
            StarSystem = celaeno,
            WarType = WarType.War,
            Status = ConflictStatus.Active
        };
        DbContext.Conflicts.Add(conflict);
        DiscordGuildPresenceGoal yellowSubmarinesControlCelaeno = new()
        {
            DiscordGuild = DiscordGuild,
            Presence = purplePeopleEastersInCelaeno,
            Goal = MaintainGoal.Instance.Name
        };
        DbContext.DiscordGuildPresenceGoals.Add(yellowSubmarinesControlCelaeno);
        DbContext.SaveChanges();

        ToDoListGenerator generator = new(DbContext);
        ToDoList toDoList = generator.Generate(DiscordGuild.GuildId);
        Assert.That(toDoList.MinorFaction, Is.EqualTo(PurplePeopleEaters.Name));
        Assert.That(toDoList.Suggestions, Is.EquivalentTo(
           new Suggestion[] {
               new ConflictSuggestion(conflict.StarSystem, conflict.MinorFaction2, conflict.MinorFaction2WonDays,
                   conflict.MinorFaction1,conflict.MinorFaction1WonDays, ConflictState.CloseDefeat, conflict.WarType)
           }).Using(DbConflictSuggestionEqualityComparer.Instance));
    }
}
