using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using OrderBot.Core;
using OrderBot.Core.Test;
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
        internal const string MinorFactionName = "Purple People Eaters";
        internal OrderBotDbContextFactory DbContextFactory { get; set; }
        internal TransactionScope TransactionScope { get; set; }
        internal OrderBotDbContext DbContext { get; set; }

        public void TestGenerate_Empty()
        {
            ToDoListGenerator generator = new(Logger, DbContextFactory);
            ToDoList toDoList = generator.Generate(MinorFactionName);
            Assert.That(toDoList.MinorFaction, Is.EqualTo(MinorFactionName));
            Assert.That(toDoList.Pro, Is.Empty);
            Assert.That(toDoList.Anti, Is.Empty);
        }

        public void TestGenerate_SingleSystem_DefaultGoal_None()
        {
            StarSystem alphCentauri = new() { Name = "Alpha Centauri", LastUpdated = DateTime.UtcNow };
            MinorFaction purplePeopleEaters = new() { Name = MinorFactionName };
            StarSystemMinorFaction starSystemMinorFaction =
                new() { MinorFaction = purplePeopleEaters, StarSystem = alphCentauri, Influence = 0.6 };
            DbContext.StarSystems.Add(alphCentauri);
            DbContext.MinorFactions.Add(purplePeopleEaters);
            DbContext.StarSystemMinorFactions.Add(starSystemMinorFaction);
            DbContext.SaveChanges();

            ToDoListGenerator generator = new(Logger, DbContextFactory);
            ToDoList toDoList = generator.Generate(MinorFactionName);
            Assert.That(toDoList.MinorFaction, Is.EqualTo(MinorFactionName));
            Assert.That(toDoList.Pro, Is.Empty);
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
