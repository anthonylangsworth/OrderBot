using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using NUnit.Framework;
using OrderBot.Core;
using OrderBot.Core.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddnMessageProcessor.Test
{
    public class TestEddnMessageSink
    {
        bool useInMemoryDB = false;

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

            IDbContextFactory<OrderBotDbContext> dbContextFactory = new OrderBotDbContextFactory(useInMemoryDB);
            DateTime newTimestamp = DateTime.UtcNow.ToUniversalTime();
            EddnMessageSink messageSink = new EddnMessageSink(dbContextFactory);
            messageSink.Sink(newTimestamp, starSystem, new MinorFactionInfo[]
            {
                new MinorFactionInfo(minorFaction, newInfluence, new string[0])
            });

            using (OrderBotDbContext dbContext = dbContextFactory.CreateDbContext())
            {
                IEnumerable<SystemMinorFaction> systemMinorFactions = dbContext.SystemMinorFaction;
                Assert.That(systemMinorFactions.Count, Is.EqualTo(1));
                SystemMinorFaction newSystemMinorFaction = systemMinorFactions.First();
                Assert.That(newSystemMinorFaction.StarSystem, Is.EqualTo(starSystem));
                Assert.That(newSystemMinorFaction.MinorFaction, Is.EqualTo(minorFaction));
                Assert.That(newSystemMinorFaction.Influence, Is.EqualTo(newInfluence));
                Assert.That(newSystemMinorFaction.States, Is.Empty);

                dbContext.SystemMinorFaction.Remove(newSystemMinorFaction);
                dbContext.SaveChanges();
            }
        }

        [Test]
        public void TestNewSystemWithStates()
        {
            string starSystem = "A";
            string minorFaction = "B";
            double newInfluence = 0.7;
            string[] states = new string[] { "C", "D" };

            IDbContextFactory<OrderBotDbContext> dbContextFactory = new OrderBotDbContextFactory(useInMemoryDB);
            DateTime newTimestamp = DateTime.UtcNow.ToUniversalTime();
            EddnMessageSink messageSink = new EddnMessageSink(dbContextFactory);
            messageSink.Sink(newTimestamp, starSystem, new MinorFactionInfo[]
            {
                new MinorFactionInfo(minorFaction, newInfluence, states)
            });

            using (OrderBotDbContext dbContext = dbContextFactory.CreateDbContext())
            {
                IEnumerable<SystemMinorFaction> systemMinorFactions = dbContext.SystemMinorFaction;
                Assert.That(systemMinorFactions.Count, Is.EqualTo(1));
                SystemMinorFaction newSystemMinorFaction = systemMinorFactions.First();
                Assert.That(newSystemMinorFaction.StarSystem, Is.EqualTo(starSystem));
                Assert.That(newSystemMinorFaction.MinorFaction, Is.EqualTo(minorFaction));
                Assert.That(newSystemMinorFaction.Influence, Is.EqualTo(newInfluence));
                Assert.That(newSystemMinorFaction.States, Is.EquivalentTo(states));

                dbContext.SystemMinorFaction.Remove(newSystemMinorFaction);
                dbContext.SaveChanges();
            }
        }

        //[Test]
        //public void TestExistingSystem()
        //{
        //    string starSystem = "A";
        //    string minorFaction = "B";
        //    double newInfluence = 0.7;

        //    IDbContextFactory<OrderBotDbContext> dbContextFactory = new OrderBotDbContextFactory();
        //    using (OrderBotDbContext dbContext = dbContextFactory.CreateDbContext())
        //    {
        //        dbContext.SystemMinorFaction.Add(new SystemMinorFaction
        //        {
        //            StarSystem = starSystem,
        //            MinorFaction = minorFaction,
        //            Influence = 0.5,
        //            LastUpdated = DateTime.UtcNow.AddHours(-1).ToUniversalTime()
        //        });
        //        dbContext.SaveChanges();
        //    }
        //}
    }
}
