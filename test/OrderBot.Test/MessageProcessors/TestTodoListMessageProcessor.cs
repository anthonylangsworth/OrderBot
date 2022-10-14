using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using OrderBot.Core;
using OrderBot.MessageProcessors;
using System.Transactions;

namespace OrderBot.Test.MessageProcessors
{
    internal class TestTodoListMessageProcessor
    {
        [Test]
        public void Ctor()
        {
            ILogger<TodoListMessageProcessor> logger = new NullLogger<TodoListMessageProcessor>();
            OrderBotDbContextFactory dbContextFactory = new();
            FixedMinorFactionNameFilter filter = new(new[] { "a" });

            TodoListMessageProcessor systemMinorFactionMessageProcessor = new(logger, dbContextFactory, filter);
            Assert.That(systemMinorFactionMessageProcessor.Logger, Is.EqualTo(logger));
            Assert.That(systemMinorFactionMessageProcessor.DbContextFactory, Is.EqualTo(dbContextFactory));
            Assert.That(systemMinorFactionMessageProcessor.Filter, Is.EqualTo(filter));
        }

        [Test]
        public void Update_NewSystem()
        {
            const string starSystem = "A";
            const string minorFaction = "B";
            const double newInfluence = 0.7;
            string[] states = new string[] { "C", "D" };
            DateTime timestamp = DateTime.UtcNow.ToUniversalTime();

            ILogger<TodoListMessageProcessor> logger = new NullLogger<TodoListMessageProcessor>();
            OrderBotDbContextFactory dbContextFactory = new();
            FixedMinorFactionNameFilter filter = new(new[] { "a" });

            using OrderBotDbContextFactory orderBotDbContextFactory = new(useInMemory: false);
            using TransactionScope transactionScope = new();
            using OrderBotDbContext dbContext = dbContextFactory.CreateDbContext();

            TodoListMessageProcessor.Update(timestamp, starSystem, new MinorFactionInfluence[]
            {
                new MinorFactionInfluence(minorFaction, newInfluence, states)
            }, dbContext);
            IEnumerable<StarSystemMinorFaction> systemMinorFactions = dbContext.StarSystemMinorFactions.Include(smf => smf.States)
                                                                                                       .Include(smf => smf.StarSystem)
                                                                                                       .Include(smf => smf.MinorFaction)
                                                                                                       .Where(smf => smf.StarSystem.Name == starSystem);
            Assert.That(systemMinorFactions.Count, Is.EqualTo(1));
            StarSystemMinorFaction? newSystemMinorFaction = systemMinorFactions.First();
            Assert.That(newSystemMinorFaction.StarSystem, Is.Not.Null);
            Assert.That(newSystemMinorFaction.StarSystem.Name, Is.EqualTo(starSystem));
            Assert.That(newSystemMinorFaction.StarSystem.LastUpdated, Is.EqualTo(timestamp).Using(DbDateTimeComparer.Instance));
            Assert.That(newSystemMinorFaction.MinorFaction, Is.Not.Null);
            Assert.That(newSystemMinorFaction.MinorFaction.Name, Is.EqualTo(minorFaction));
            Assert.That(newSystemMinorFaction.Influence, Is.EqualTo(newInfluence));
            Assert.That(newSystemMinorFaction.States.Select(state => state.Name), Is.EquivalentTo(states));
        }

        [Test]
        public void Update_TwoSystems()
        {
            string starSystem1 = "A";
            string starSystem2 = "B";
            MinorFactionInfluence systemOneMinorFactionInfo = new("MF1", 0.3, new string[] { "A", "B" });
            MinorFactionInfluence systemTwoMinorFactionInfo = new("MF2", 0.5, new string[] { "B" }); string[] states = new string[] { "C", "D" };
            DateTime timestamp = DateTime.UtcNow.ToUniversalTime();

            ILogger<TodoListMessageProcessor> logger = new NullLogger<TodoListMessageProcessor>();
            OrderBotDbContextFactory dbContextFactory = new();
            FixedMinorFactionNameFilter filter = new(new[] { "a" });

            using OrderBotDbContextFactory orderBotDbContextFactory = new(useInMemory: false);
            using TransactionScope transactionScope = new();
            using OrderBotDbContext dbContext = dbContextFactory.CreateDbContext();

            TodoListMessageProcessor.Update(timestamp, starSystem1, new MinorFactionInfluence[]
            {
                systemOneMinorFactionInfo
            }, dbContext);
            TodoListMessageProcessor.Update(timestamp, starSystem2, new MinorFactionInfluence[]
            {
                systemTwoMinorFactionInfo
            }, dbContext);
            List<StarSystemMinorFaction> systemMinorFactions = dbContext.StarSystemMinorFactions.Include(smf => smf.States)
                                                                                                .Include(smf => smf.StarSystem)
                                                                                                .Include(smf => smf.MinorFaction)
                                                                                                .Where(smf => smf.StarSystem.Name == starSystem1 || smf.StarSystem.Name == starSystem2)
                                                                                                .ToList();
            Assert.That(systemMinorFactions.Count, Is.EqualTo(2));
            Assert.That(Helpers.IsSame(systemMinorFactions[0], starSystem1, timestamp, systemOneMinorFactionInfo), Is.True);
            Assert.That(Helpers.IsSame(systemMinorFactions[1], starSystem2, timestamp, systemTwoMinorFactionInfo), Is.True);
        }
    }
}
