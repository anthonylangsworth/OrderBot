using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using NUnit.Framework;
using OrderBot.Core;
using OrderBot.Core.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace EddnMessageProcessor.Test
{
    public class TestEddnMessageSink
    {
        bool useInMemoryDB = true;

        [Test]
        public void Ctor()
        {
            IDbContextFactory<OrderBotDbContext> dbContextFactory = new OrderBotDbContextFactory(useInMemoryDB);
            EddnMessageSink messageSink = new EddnMessageSink(dbContextFactory);
            Assert.That(messageSink.DbContextFactory, Is.EqualTo(dbContextFactory));
        }

        [Test]
        public void TestNewSystemWithNoStates()
        {
            string starSystem = "A";
            string minorFaction = "B";
            double newInfluence = 0.7;

            using OrderBotDbContextFactory dbContextFactory = new OrderBotDbContextFactory(useInMemoryDB);
            using IDbContextTransaction transaction = dbContextFactory.BeginTransaction();
            DateTime timestamp = DateTime.UtcNow.ToUniversalTime();
            EddnMessageSink messageSink = new EddnMessageSink(dbContextFactory);

            messageSink.Sink(timestamp, starSystem, new MinorFactionInfo[]
            {
                new MinorFactionInfo(minorFaction, newInfluence, new string[0])
            });

            using (OrderBotDbContext dbContext = dbContextFactory.CreateDbContext())
            {
                IEnumerable<StarSystemMinorFaction> systemMinorFactions = dbContext.StarSystemMinorFactions.Include(smf => smf.States)
                                                                                                           .Include(smf => smf.StarSystem)
                                                                                                           .Include(smf => smf.MinorFaction);
                Assert.That(systemMinorFactions.Count, Is.EqualTo(1));
                StarSystemMinorFaction? newSystemMinorFaction = systemMinorFactions.First();
                Assert.That(newSystemMinorFaction.StarSystem, Is.Not.Null);
                Assert.That(newSystemMinorFaction.StarSystem.Name, Is.EqualTo(starSystem));
                Assert.That(newSystemMinorFaction.StarSystem.LastUpdated, Is.EqualTo(timestamp).Using(DbDateTimeComparer.Instance));
                Assert.That(newSystemMinorFaction.MinorFaction, Is.Not.Null);
                Assert.That(newSystemMinorFaction.MinorFaction.Name, Is.EqualTo(minorFaction));
                Assert.That(newSystemMinorFaction.Influence, Is.EqualTo(newInfluence));
                Assert.That(newSystemMinorFaction.States, Is.Empty);
            }
        }

        [Test]
        public void TestNewSystemWithStates()
        {
            string starSystem = "A";
            string minorFaction = "B";
            double newInfluence = 0.7;
            string[] states = new string[] { "C", "D" };

            using OrderBotDbContextFactory dbContextFactory = new OrderBotDbContextFactory(useInMemoryDB);
            using IDbContextTransaction transaction = dbContextFactory.BeginTransaction();
            DateTime timestamp = DateTime.UtcNow.ToUniversalTime();
            EddnMessageSink messageSink = new EddnMessageSink(dbContextFactory);

            messageSink.Sink(timestamp, starSystem, new MinorFactionInfo[]
            {
                new MinorFactionInfo(minorFaction, newInfluence, states)
            });

            using (OrderBotDbContext dbContext = dbContextFactory.CreateDbContext())
            {
                IEnumerable<StarSystemMinorFaction> systemMinorFactions = dbContext.StarSystemMinorFactions.Include(smf => smf.States)
                                                                                                           .Include(smf => smf.StarSystem)
                                                                                                           .Include(smf => smf.MinorFaction);
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
        }

        [Test]
        public void TestExistingSystemOneMinorFaction()
        {
            using OrderBotDbContextFactory dbContextFactory = new OrderBotDbContextFactory(useInMemoryDB);
            using IDbContextTransaction transaction = dbContextFactory.BeginTransaction();
            EddnMessageSink messageSink = new EddnMessageSink(dbContextFactory);

            string starSystem = "A";
            string minorFaction = "B";
            MinorFactionInfo minorFactionInfo1 = new MinorFactionInfo(minorFaction, 0.2, new string[] { "A", "B" });
            MinorFactionInfo minorFactionInfo2 = new MinorFactionInfo(minorFaction, 0.5, new string[] { "B", "C" });
            DateTime timestamp1 = DateTime.UtcNow.AddSeconds(-1).ToUniversalTime();
            DateTime timestamp2 = DateTime.UtcNow.ToUniversalTime();
            messageSink.Sink(timestamp1, starSystem, new MinorFactionInfo[]
            {
                minorFactionInfo1
            });
            messageSink.Sink(timestamp2, starSystem, new MinorFactionInfo[]
            {
                minorFactionInfo2
            });

            using (OrderBotDbContext dbContext = dbContextFactory.CreateDbContext())
            {
                IEnumerable<StarSystemMinorFaction> systemMinorFactions = dbContext.StarSystemMinorFactions.Include(smf => smf.States)
                                                                                                           .Include(smf => smf.StarSystem)
                                                                                                           .Include(smf => smf.MinorFaction);
                Assert.That(systemMinorFactions.Count, Is.EqualTo(1));
                StarSystemMinorFaction newSystemMinorFaction = systemMinorFactions.First();
                Assert.That(Helpers.IsSame(newSystemMinorFaction, starSystem, minorFactionInfo2), Is.True);
            }
        }

        [Test]
        public void TestExistingSystemMultipleMinorFactions()
        {
            using OrderBotDbContextFactory dbContextFactory = new OrderBotDbContextFactory(useInMemoryDB);
            using IDbContextTransaction transaction = dbContextFactory.BeginTransaction();
            EddnMessageSink messageSink = new EddnMessageSink(dbContextFactory);

            string starSystem = "A";
            MinorFactionInfo oldMinorFactionInfo1 = new MinorFactionInfo("A", 0.2, new string[] { "A", "B" });
            MinorFactionInfo oldMinorFactionInfo2 = new MinorFactionInfo("B", 0.5, new string[] { "B", "C" });
            MinorFactionInfo newMinorFactionInfo1 = new MinorFactionInfo("B", 0.6, new string[] { "B" });
            MinorFactionInfo newMinorFactionInfo2 = new MinorFactionInfo("C", 0.1, new string[] { "D", "E", "F" });
            DateTime timestamp1 = DateTime.UtcNow.AddSeconds(-1).ToUniversalTime();
            DateTime timestamp2 = DateTime.UtcNow.ToUniversalTime();
            messageSink.Sink(timestamp1, starSystem, new MinorFactionInfo[]
            {
                oldMinorFactionInfo1,
                oldMinorFactionInfo2
            });
            messageSink.Sink(timestamp2, starSystem, new MinorFactionInfo[]
            {
                newMinorFactionInfo1,
                newMinorFactionInfo2
            });

            using (OrderBotDbContext dbContext = dbContextFactory.CreateDbContext())
            {
                IEnumerable<StarSystemMinorFaction> systemMinorFactions = dbContext.StarSystemMinorFactions.Include(smf => smf.States)
                                                                                                           .Include(smf => smf.StarSystem)
                                                                                                           .Include(smf => smf.MinorFaction);
                Assert.That(systemMinorFactions.Count, Is.EqualTo(2));
                Assert.That(Helpers.IsSame(systemMinorFactions.First(), starSystem, newMinorFactionInfo1), Is.True);
                Assert.That(Helpers.IsSame(systemMinorFactions.Skip(1).First(), starSystem, newMinorFactionInfo2), Is.True);
            }
        }
    }
}
