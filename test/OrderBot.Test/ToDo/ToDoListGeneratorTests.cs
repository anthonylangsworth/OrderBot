using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using OrderBot.Core;
using OrderBot.EntityFramework;
using OrderBot.ToDo;
using System.Transactions;

namespace OrderBot.Test.ToDo
{
    internal class ToDoListGeneratorTests
    {
        [SetUp]
        public void SetUp()
        {
            TearDown();
            DbContextFactory = new(useInMemory: false);
            TransactionScope = new();
            DbContext = DbContextFactory.CreateDbContext();
        }

        [TearDown]
        public void TearDown()
        {
            DbContext?.Dispose();
            TransactionScope?.Dispose();
            DbContextFactory?.Dispose();
        }

        internal ILogger<ToDoListGenerator> Logger = new NullLogger<ToDoListGenerator>();
        internal const ulong GuildId = 382284915670253569;
        internal const string MinorFactionName = "Purple People Eaters";
        internal OrderBotDbContextFactory DbContextFactory { get; set; } = null!;
        internal TransactionScope TransactionScope { get; set; } = null!;
        internal OrderBotDbContext DbContext { get; set; } = null!;

        [Test]
        public void Ctor()
        {
            ToDoListGenerator generator = new(Logger, DbContextFactory);
            Assert.That(generator.Logger, Is.EqualTo(Logger));
            Assert.That(generator.DbContextFactory, Is.EqualTo(DbContextFactory));
        }

        [Test]
        public void Generate_Empty()
        {
            ToDoListGenerator generator = new(Logger, DbContextFactory);
            ToDoList toDoList = generator.Generate(GuildId, MinorFactionName);
            Assert.That(toDoList.MinorFaction, Is.EqualTo(MinorFactionName));
            Assert.That(toDoList.Suggestions, Is.Empty);
        }

        [Test]
        public void Generate_SingleSystem_DefaultGoal_None()
        {
            StarSystem alphCentauri = new() { Name = "Alpha Centauri", LastUpdated = DateTime.UtcNow };
            MinorFaction purplePeopleEaters = new() { Name = MinorFactionName };
            Presence starSystemMinorFaction =
                new()
                {
                    MinorFaction = purplePeopleEaters,
                    StarSystem = alphCentauri,
                    Influence = (ControlGoal.LowerInfluenceThreshold + ControlGoal.UpperInfluenceThreshold) / 2
                };
            DiscordGuildPresenceGoal discordGuild = new()
            {
                DiscordGuild = new DiscordGuild() { GuildId = GuildId },
                Presence = starSystemMinorFaction,
                Goal = Goals.Default.Name
            };
            DbContext.DiscordGuildPresenceGoals.Add(discordGuild);
            DbContext.SaveChanges();

            ToDoListGenerator generator = new(Logger, DbContextFactory);
            ToDoList toDoList = generator.Generate(GuildId, MinorFactionName);
            Assert.That(toDoList.MinorFaction, Is.EqualTo(MinorFactionName));
            Assert.That(toDoList.Suggestions, Is.Empty);
        }

        [Test]
        public void Generate_SingleSystem_DefaultGoal_Pro()
        {
            StarSystem alphCentauri = new() { Name = "Alpha Centauri", LastUpdated = DateTime.UtcNow };
            MinorFaction purplePeopleEaters = new() { Name = MinorFactionName };
            Presence starSystemMinorFaction =
                new() { MinorFaction = purplePeopleEaters, StarSystem = alphCentauri, Influence = ControlGoal.LowerInfluenceThreshold - 0.01 };
            DiscordGuildPresenceGoal discordGuild = new()
            {
                DiscordGuild = new() { GuildId = GuildId },
                Presence = starSystemMinorFaction,
                Goal = Goals.Default.Name
            };
            DbContext.DiscordGuildPresenceGoals.Add(discordGuild);
            DbContext.SaveChanges();

            ToDoListGenerator generator = new(Logger, DbContextFactory);
            ToDoList toDoList = generator.Generate(GuildId, MinorFactionName);
            Assert.That(toDoList.MinorFaction, Is.EqualTo(MinorFactionName));
            Assert.That(toDoList.Suggestions,
                Is.EquivalentTo(new[] { new InfluenceSuggestion() { StarSystem = alphCentauri, Influence = starSystemMinorFaction.Influence, Pro = true } })
                  .Using(DbInfluenceInitiatedSuggestionEqualityComparer.Instance));
        }

        [Test]
        public void Generate_SingleSystem_DefaultGoal_Anti()
        {
            StarSystem alphCentauri = new() { Name = "Alpha Centauri", LastUpdated = DateTime.UtcNow };
            MinorFaction purplePeopleEaters = new() { Name = MinorFactionName };
            Presence starSystemMinorFaction =
                new() { MinorFaction = purplePeopleEaters, StarSystem = alphCentauri, Influence = ControlGoal.UpperInfluenceThreshold + 0.01 };
            DiscordGuildPresenceGoal discordGuild = new()
            {
                DiscordGuild = new() { GuildId = GuildId },
                Presence = starSystemMinorFaction,
                Goal = Goals.Default.Name
            };
            DbContext.DiscordGuildPresenceGoals.Add(discordGuild);
            DbContext.SaveChanges();

            ToDoListGenerator generator = new(Logger, DbContextFactory);
            ToDoList toDoList = generator.Generate(GuildId, MinorFactionName);
            Assert.That(toDoList.MinorFaction, Is.EqualTo(MinorFactionName));
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
                DiscordGuild = new DiscordGuild() { GuildId = GuildId },
                Presence = starSystemMinorFaction,
                Goal = Goals.Default.Name
            };
            DbContext.DiscordGuildPresenceGoals.Add(discordGuild);
            DbContext.SaveChanges();

            ToDoListGenerator generator = new(Logger, DbContextFactory);
            ToDoList toDoList = generator.Generate(GuildId, MinorFactionName);
            Assert.That(toDoList.MinorFaction, Is.EqualTo(MinorFactionName));
            Assert.That(toDoList.Suggestions, Is.Empty);
        }

        [Test]
        public void Generate_MultipleSystems_DefaultGoal()
        {
            StarSystem alphCentauri = new() { Name = "Alpha Centauri", LastUpdated = DateTime.UtcNow };
            StarSystem maia = new() { Name = "Maia", LastUpdated = DateTime.UtcNow };
            MinorFaction purplePeopleEaters = new() { Name = MinorFactionName };
            DiscordGuild discordGuild = new() { GuildId = GuildId };
            DiscordGuildPresenceGoal purplePeopleEastersAlphaCentauri = new()
            {
                DiscordGuild = discordGuild,
                Presence = new() { MinorFaction = purplePeopleEaters, StarSystem = alphCentauri, Influence = ControlGoal.LowerInfluenceThreshold - 0.01 },
                Goal = Goals.Default.Name
            };
            DbContext.DiscordGuildPresenceGoals.Add(purplePeopleEastersAlphaCentauri);
            DiscordGuildPresenceGoal purplePeopleEastersMaia = new()
            {
                DiscordGuild = discordGuild,
                Presence = new() { MinorFaction = purplePeopleEaters, StarSystem = maia, Influence = ControlGoal.UpperInfluenceThreshold + 0.01 },
                Goal = Goals.Default.Name
            };
            DbContext.DiscordGuildPresenceGoals.Add(purplePeopleEastersMaia);
            DbContext.SaveChanges();

            ToDoListGenerator generator = new(Logger, DbContextFactory);
            ToDoList toDoList = generator.Generate(GuildId, MinorFactionName);
            Assert.That(toDoList.MinorFaction, Is.EqualTo(MinorFactionName));
            Assert.That(toDoList.Suggestions, Is.EquivalentTo(
               new Suggestion[] {
                   new InfluenceSuggestion() { StarSystem = alphCentauri, Influence = purplePeopleEastersAlphaCentauri.Presence.Influence, Pro = true },
                   new InfluenceSuggestion() { StarSystem = maia, Influence = purplePeopleEastersMaia.Presence.Influence, Pro = false }
               }).Using(DbInfluenceInitiatedSuggestionEqualityComparer.Instance));
        }

        // Generate sample report on current DB
        [Test]
        public void Generate()
        {
            ToDoListGenerator generator = new(Logger, DbContextFactory);
            ToDoList toDoList = generator.Generate(GuildId, "EDA Kunti League");
        }
    }
}
