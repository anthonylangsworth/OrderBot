using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using OrderBot.Core;
using OrderBot.Reports;
using System.Transactions;

namespace OrderBot.Test.Reports
{
    internal class TestToDoListGenerator
    {
        public TestToDoListGenerator()
        {
            DbContextFactory = new(useInMemory: false);
            TransactionScope = new();
            DbContext = DbContextFactory.CreateDbContext();
        }

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
        internal const string Snowflake = "ABCDEF12345";
        internal const string MinorFactionName = "Purple People Eaters";
        internal OrderBotDbContextFactory DbContextFactory { get; set; }
        internal TransactionScope TransactionScope { get; set; }
        internal OrderBotDbContext DbContext { get; set; }

        [Test]
        public void Generate_Empty()
        {
            ToDoListGenerator generator = new(Logger, DbContextFactory);
            ToDoList toDoList = generator.Generate(Snowflake, MinorFactionName);
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
                DiscordGuild = new DiscordGuild() { Snowflake = Snowflake },
                StarSystemMinorFaction = starSystemMinorFaction
            };
            DbContext.DiscordGuildStarSystemMinorFactionGoals.Add(discordGuild);
            DbContext.SaveChanges();

            ToDoListGenerator generator = new(Logger, DbContextFactory);
            ToDoList toDoList = generator.Generate(Snowflake, MinorFactionName);
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
                new() { MinorFaction = purplePeopleEaters, StarSystem = alphCentauri, Influence = ControlGoal.LowerInfluenceThreshold - 0.1 };
            DiscordGuildStarSystemMinorFactionGoal discordGuild = new()
            {
                DiscordGuild = new() { Snowflake = Snowflake },
                StarSystemMinorFaction = starSystemMinorFaction
            };
            DbContext.DiscordGuildStarSystemMinorFactionGoals.Add(discordGuild);
            DbContext.SaveChanges();

            ToDoListGenerator generator = new(Logger, DbContextFactory);
            ToDoList toDoList = generator.Generate(Snowflake, MinorFactionName);
            Assert.That(toDoList.MinorFaction, Is.EqualTo(MinorFactionName));
            Assert.That(toDoList.Pro,
                Is.EquivalentTo(new[] { new InfluenceInitiatedAction() { StarSystem = alphCentauri, Influence = starSystemMinorFaction.Influence } })
                  .Using(DbInfluenceInitiatedActionEqualityComparer.Instance));
            Assert.That(toDoList.Anti, Is.Empty);
        }

        //[Test]
        //public void TestGenerate_Complex()
        //{
        //    DbContext.StarSystems.Add(new StarSystem() { Name = "Alpha Centauri", LastUpdated = DateTime.UtcNow });
        //    DbContext.StarSystems.Add(new StarSystem() { Name = "Sol", LastUpdated = DateTime.UtcNow });
        //    DbContext.SaveChanges();

        //    DbContext.MinorFactions.Add(new MinorFaction() { Name = MinorFactionName });
        //    DbContext.MinorFactions.Add(new MinorFaction() { Name = "Puff the Magic Dragons" });

        //    ToDoListGenerator generator = new(logger, dbContextFactory);
        //    ToDoList toDoList = generator.Generate("EDA Kunti League");
        //    Assert.That(toDoList.Pro, Is.Not.Null);
        //}
    }
}
