using NUnit.Framework;
using OrderBot.Core;
using OrderBot.EntityFramework;
using OrderBot.ToDo;
using System.Transactions;

namespace OrderBot.Test.ToDo;

internal class ToDoListGeneratorTests
{
    public ToDoListGeneratorTests()
    {
        DbContextFactory = new(useInMemory: false);
        DbContext = DbContextFactory.CreateDbContext();
    }

    [SetUp]
    public void SetUp()
    {
        TearDown();
        TransactionScope = new();
        DiscordGuild = new DiscordGuild() { GuildId = 382284915670253569, Name = "Test Guild" };
        PurplePeopleEaters = new MinorFaction() { Name = "Purple People Eaters" };
        DbContext.MinorFactions.Add(PurplePeopleEaters);
        DbContext.DiscordGuilds.Add(DiscordGuild);
        DbContext.SaveChanges();
        DiscordGuild.SupportedMinorFactions.Add(PurplePeopleEaters);
        DbContext.SaveChanges();
    }

    [TearDown]
    public void TearDown()
    {
        // Intentionally do not Compelete(), ensuring changes are rolled back
        TransactionScope?.Dispose();
    }

    internal DiscordGuild DiscordGuild { get; set; } = null!;
    internal MinorFaction PurplePeopleEaters { get; set; } = null!;
    internal OrderBotDbContextFactory DbContextFactory { get; }
    internal TransactionScope TransactionScope { get; set; } = null!;
    internal OrderBotDbContext DbContext { get; }

    [Test]
    public void Ctor()
    {
        ToDoListGenerator generator = new(DbContextFactory);
        Assert.That(generator.DbContextFactory, Is.EqualTo(DbContextFactory));
    }

    [Test]
    public void Generate_Empty()
    {
        ToDoListGenerator generator = new(DbContextFactory);
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

        ToDoListGenerator generator = new(DbContextFactory);
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

        ToDoListGenerator generator = new(DbContextFactory);
        ToDoList toDoList = generator.Generate(DiscordGuild.GuildId);
        Assert.That(toDoList.MinorFaction, Is.EqualTo(PurplePeopleEaters.Name));
        Assert.That(toDoList.Suggestions,
            Is.EquivalentTo(new[] { new InfluenceSuggestion() { StarSystem = alphCentauri, Influence = starSystemMinorFaction.Influence, Pro = true } })
              .Using(DbInfluenceInitiatedSuggestionEqualityComparer.Instance));
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

        ToDoListGenerator generator = new(DbContextFactory);
        ToDoList toDoList = generator.Generate(DiscordGuild.GuildId);
        Assert.That(toDoList.MinorFaction, Is.EqualTo(PurplePeopleEaters.Name));
        Assert.That(toDoList.Suggestions,
            Is.EquivalentTo(new[] { new InfluenceSuggestion() { StarSystem = alphCentauri, Influence = starSystemMinorFaction.Influence, Pro = true } })
              .Using(DbInfluenceInitiatedSuggestionEqualityComparer.Instance));
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

        ToDoListGenerator generator = new(DbContextFactory);
        ToDoList toDoList = generator.Generate(DiscordGuild.GuildId);
        Assert.That(toDoList.MinorFaction, Is.EqualTo(PurplePeopleEaters.Name));
        Assert.That(toDoList.Suggestions, Is.Empty);
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

        ToDoListGenerator generator = new(DbContextFactory);
        ToDoList toDoList = generator.Generate(DiscordGuild.GuildId);
        Assert.That(toDoList.MinorFaction, Is.EqualTo(PurplePeopleEaters.Name));
        Assert.That(toDoList.Suggestions, Is.EquivalentTo(
           new Suggestion[] {
               new InfluenceSuggestion() { StarSystem = alphCentauri, Influence = purplePeopleEastersAlphaCentauri.Presence.Influence, Pro = true },
               new InfluenceSuggestion() { StarSystem = maia, Influence = purplePeopleEastersMaia.Presence.Influence, Pro = false }
           }).Using(DbInfluenceInitiatedSuggestionEqualityComparer.Instance));
    }
}
