using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using OrderBot.Core;
using OrderBot.EntityFramework;
using OrderBot.ToDo;
using System.Transactions;

namespace OrderBot.Test.ToDo
{
    internal class TestToDoListGenerator
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
            Assert.That(toDoList.Pro, Is.Empty);
            Assert.That(toDoList.Anti, Is.Empty);
        }

        [Test]
        public void Generate_SingleSystem_DefaultGoal_None()
        {
            StarSystem alphCentauri = new() { Name = "Alpha Centauri", LastUpdated = DateTime.UtcNow };
            MinorFaction purplePeopleEaters = new() { Name = MinorFactionName };
            StarSystemMinorFaction starSystemMinorFaction =
                new()
                {
                    MinorFaction = purplePeopleEaters,
                    StarSystem = alphCentauri,
                    Influence = (ControlGoal.LowerInfluenceThreshold + ControlGoal.UpperInfluenceThreshold) / 2
                };
            DiscordGuildStarSystemMinorFactionGoal discordGuild = new()
            {
                DiscordGuild = new DiscordGuild() { GuildId = GuildId },
                StarSystemMinorFaction = starSystemMinorFaction,
                Goal = Goals.Default.Name
            };
            DbContext.DiscordGuildStarSystemMinorFactionGoals.Add(discordGuild);
            DbContext.SaveChanges();

            ToDoListGenerator generator = new(Logger, DbContextFactory);
            ToDoList toDoList = generator.Generate(GuildId, MinorFactionName);
            Assert.That(toDoList.MinorFaction, Is.EqualTo(MinorFactionName));
            Assert.That(toDoList.Pro, Is.Empty);
            Assert.That(toDoList.Anti, Is.Empty);
        }

        [Test]
        public void Generate_SingleSystem_DefaultGoal_Pro()
        {
            StarSystem alphCentauri = new() { Name = "Alpha Centauri", LastUpdated = DateTime.UtcNow };
            MinorFaction purplePeopleEaters = new() { Name = MinorFactionName };
            StarSystemMinorFaction starSystemMinorFaction =
                new() { MinorFaction = purplePeopleEaters, StarSystem = alphCentauri, Influence = ControlGoal.LowerInfluenceThreshold - 0.01 };
            DiscordGuildStarSystemMinorFactionGoal discordGuild = new()
            {
                DiscordGuild = new() { GuildId = GuildId },
                StarSystemMinorFaction = starSystemMinorFaction,
                Goal = Goals.Default.Name
            };
            DbContext.DiscordGuildStarSystemMinorFactionGoals.Add(discordGuild);
            DbContext.SaveChanges();

            ToDoListGenerator generator = new(Logger, DbContextFactory);
            ToDoList toDoList = generator.Generate(GuildId, MinorFactionName);
            Assert.That(toDoList.MinorFaction, Is.EqualTo(MinorFactionName));
            Assert.That(toDoList.Pro,
                Is.EquivalentTo(new[] { new InfluenceSuggestion() { StarSystem = alphCentauri, Influence = starSystemMinorFaction.Influence } })
                  .Using(DbInfluenceInitiatedSuggestionEqualityComparer.Instance));
            Assert.That(toDoList.Anti, Is.Empty);
        }

        [Test]
        public void Generate_SingleSystem_DefaultGoal_Anti()
        {
            StarSystem alphCentauri = new() { Name = "Alpha Centauri", LastUpdated = DateTime.UtcNow };
            MinorFaction purplePeopleEaters = new() { Name = MinorFactionName };
            StarSystemMinorFaction starSystemMinorFaction =
                new() { MinorFaction = purplePeopleEaters, StarSystem = alphCentauri, Influence = ControlGoal.UpperInfluenceThreshold + 0.01 };
            DiscordGuildStarSystemMinorFactionGoal discordGuild = new()
            {
                DiscordGuild = new() { GuildId = GuildId },
                StarSystemMinorFaction = starSystemMinorFaction,
                Goal = Goals.Default.Name
            };
            DbContext.DiscordGuildStarSystemMinorFactionGoals.Add(discordGuild);
            DbContext.SaveChanges();

            ToDoListGenerator generator = new(Logger, DbContextFactory);
            ToDoList toDoList = generator.Generate(GuildId, MinorFactionName);
            Assert.That(toDoList.MinorFaction, Is.EqualTo(MinorFactionName));
            Assert.That(toDoList.Pro, Is.Empty);
            Assert.That(toDoList.Anti,
                Is.EquivalentTo(new[] { new InfluenceSuggestion() { StarSystem = alphCentauri, Influence = starSystemMinorFaction.Influence } })
                  .Using(DbInfluenceInitiatedSuggestionEqualityComparer.Instance));
        }

        [Test]
        public void Generate_SingleSystem_DefaultGoal_Unrelated()
        {
            string differentMinorFactionName = "Star Gazers";
            StarSystem alphCentauri = new() { Name = "Alpha Centauri", LastUpdated = DateTime.UtcNow };
            MinorFaction purplePeopleEaters = new() { Name = differentMinorFactionName };
            StarSystemMinorFaction starSystemMinorFaction =
                new() { MinorFaction = purplePeopleEaters, StarSystem = alphCentauri, Influence = 0 };
            DiscordGuildStarSystemMinorFactionGoal discordGuild = new()
            {
                DiscordGuild = new DiscordGuild() { GuildId = GuildId },
                StarSystemMinorFaction = starSystemMinorFaction,
                Goal = Goals.Default.Name
            };
            DbContext.DiscordGuildStarSystemMinorFactionGoals.Add(discordGuild);
            DbContext.SaveChanges();

            ToDoListGenerator generator = new(Logger, DbContextFactory);
            ToDoList toDoList = generator.Generate(GuildId, MinorFactionName);
            Assert.That(toDoList.MinorFaction, Is.EqualTo(MinorFactionName));
            Assert.That(toDoList.Pro, Is.Empty);
            Assert.That(toDoList.Anti, Is.Empty);
        }

        [Test]
        public void Generate_MultipleSystems_DefaultGoal()
        {
            StarSystem alphCentauri = new() { Name = "Alpha Centauri", LastUpdated = DateTime.UtcNow };
            StarSystem maia = new() { Name = "Maia", LastUpdated = DateTime.UtcNow };
            MinorFaction purplePeopleEaters = new() { Name = MinorFactionName };
            DiscordGuild discordGuild = new() { GuildId = GuildId };
            DiscordGuildStarSystemMinorFactionGoal purplePeopleEastersAlphaCentauri = new()
            {
                DiscordGuild = discordGuild,
                StarSystemMinorFaction = new() { MinorFaction = purplePeopleEaters, StarSystem = alphCentauri, Influence = ControlGoal.LowerInfluenceThreshold - 0.01 },
                Goal = Goals.Default.Name
            };
            DbContext.DiscordGuildStarSystemMinorFactionGoals.Add(purplePeopleEastersAlphaCentauri);
            DiscordGuildStarSystemMinorFactionGoal purplePeopleEastersMaia = new()
            {
                DiscordGuild = discordGuild,
                StarSystemMinorFaction = new() { MinorFaction = purplePeopleEaters, StarSystem = maia, Influence = ControlGoal.UpperInfluenceThreshold + 0.01 },
                Goal = Goals.Default.Name
            };
            DbContext.DiscordGuildStarSystemMinorFactionGoals.Add(purplePeopleEastersMaia);
            DbContext.SaveChanges();

            ToDoListGenerator generator = new(Logger, DbContextFactory);
            ToDoList toDoList = generator.Generate(GuildId, MinorFactionName);
            Assert.That(toDoList.MinorFaction, Is.EqualTo(MinorFactionName));
            Assert.That(toDoList.Pro,
                Is.EquivalentTo(new[] { new InfluenceSuggestion() { StarSystem = alphCentauri, Influence = purplePeopleEastersAlphaCentauri.StarSystemMinorFaction.Influence } })
                  .Using(DbInfluenceInitiatedSuggestionEqualityComparer.Instance));
            Assert.That(toDoList.Anti,
                Is.EquivalentTo(new[] { new InfluenceSuggestion() { StarSystem = maia, Influence = purplePeopleEastersMaia.StarSystemMinorFaction.Influence } })
                  .Using(DbInfluenceInitiatedSuggestionEqualityComparer.Instance));
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
