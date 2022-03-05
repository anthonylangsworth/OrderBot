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
            DateTime timestamp = DateTime.UtcNow.ToUniversalTime();
            EddnMessageSink messageSink = new EddnMessageSink(dbContextFactory);

            messageSink.Sink(timestamp, starSystem, new MinorFactionInfo[]
            {
                new MinorFactionInfo(minorFaction, newInfluence, new string[0])
            });

            using (OrderBotDbContext dbContext = dbContextFactory.CreateDbContext())
            {
                IEnumerable<StarSystemMinorFaction> systemMinorFactions = dbContext.SystemMinorFactions.Include(smf => smf.State)
                                                                                                       .Include(smf => smf.StarSystem)
                                                                                                       .Include(smf => smf.MinorFaction);
                StarSystemMinorFaction? newSystemMinorFaction = null;
                try
                {
                    Assert.That(systemMinorFactions.Count, Is.EqualTo(1));
                    newSystemMinorFaction = systemMinorFactions.First();
                    Assert.That(newSystemMinorFaction.StarSystem, Is.Not.Null);
                    Assert.That(newSystemMinorFaction.StarSystem.Name, Is.EqualTo(starSystem));
                    Assert.That(newSystemMinorFaction.StarSystem.LastUpdated, Is.EqualTo(timestamp).Using(DbDateTimeComparer.Instance));
                    Assert.That(newSystemMinorFaction.MinorFaction, Is.Not.Null);
                    Assert.That(newSystemMinorFaction.MinorFaction.Name, Is.EqualTo(minorFaction));
                    Assert.That(newSystemMinorFaction.Influence, Is.EqualTo(newInfluence));
                    Assert.That(newSystemMinorFaction.State, Is.Empty);
                }
                finally
                {
                    if (newSystemMinorFaction != null)
                    {
                        dbContext.SystemMinorFactions.Remove(newSystemMinorFaction);
                        dbContext.StarSystems.Remove(dbContext.StarSystems.First(ss => ss.Name == starSystem));
                        dbContext.MinorFactions.Remove(dbContext.MinorFactions.First(mf => mf.Name == minorFaction));
                    }
                    dbContext.SaveChanges();
                }
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
            DateTime timestamp = DateTime.UtcNow.ToUniversalTime();
            EddnMessageSink messageSink = new EddnMessageSink(dbContextFactory);

            messageSink.Sink(timestamp, starSystem, new MinorFactionInfo[]
            {
                new MinorFactionInfo(minorFaction, newInfluence, states)
            });

            using (OrderBotDbContext dbContext = dbContextFactory.CreateDbContext())
            {
                IEnumerable<StarSystemMinorFaction> systemMinorFactions = dbContext.SystemMinorFactions.Include(smf => smf.State)
                                                                                                       .Include(smf => smf.StarSystem)
                                                                                                       .Include(smf => smf.MinorFaction);
                StarSystemMinorFaction? newSystemMinorFaction = null;
                try
                {
                    Assert.That(systemMinorFactions.Count, Is.EqualTo(1));
                    newSystemMinorFaction = systemMinorFactions.First();
                    Assert.That(newSystemMinorFaction.StarSystem, Is.Not.Null);
                    Assert.That(newSystemMinorFaction.StarSystem.Name, Is.EqualTo(starSystem));
                    Assert.That(newSystemMinorFaction.StarSystem.LastUpdated, Is.EqualTo(timestamp).Using(DbDateTimeComparer.Instance));
                    Assert.That(newSystemMinorFaction.MinorFaction, Is.Not.Null);
                    Assert.That(newSystemMinorFaction.MinorFaction.Name, Is.EqualTo(minorFaction));
                    Assert.That(newSystemMinorFaction.Influence, Is.EqualTo(newInfluence));
                    Assert.That(newSystemMinorFaction.State.Select(state => state.Name), Is.EquivalentTo(states));
                }
                finally
                {
                    if (newSystemMinorFaction != null)
                    {
                        dbContext.SystemMinorFactions.Remove(newSystemMinorFaction);
                        dbContext.StarSystems.Remove(dbContext.StarSystems.First(ss => ss.Name == starSystem));
                        dbContext.MinorFactions.Remove(dbContext.MinorFactions.First(mf => mf.Name == minorFaction));
                    }
                    dbContext.SaveChanges();
                }
            }
        }

        [Test]
        public void TestExistingSystemOneMinorFaction()
        {
            IDbContextFactory<OrderBotDbContext> dbContextFactory = new OrderBotDbContextFactory(useInMemoryDB);
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
                IEnumerable<StarSystemMinorFaction> systemMinorFactions = dbContext.SystemMinorFactions.Include(smf => smf.State)
                                                                                                       .Include(smf => smf.StarSystem)
                                                                                                       .Include(smf => smf.MinorFaction);
                StarSystemMinorFaction? newSystemMinorFaction = null;
                try
                {
                    Assert.That(systemMinorFactions.Count, Is.EqualTo(1));
                    newSystemMinorFaction = systemMinorFactions.First();
                    Assert.That(newSystemMinorFaction.StarSystem, Is.Not.Null);
                    Assert.That(newSystemMinorFaction.StarSystem.Name, Is.EqualTo(starSystem));
                    Assert.That(newSystemMinorFaction.StarSystem.LastUpdated, Is.EqualTo(timestamp2).Using(DbDateTimeComparer.Instance));
                    Assert.That(newSystemMinorFaction.MinorFaction, Is.Not.Null);
                    Assert.That(newSystemMinorFaction.MinorFaction.Name, Is.EqualTo(minorFaction));
                    Assert.That(newSystemMinorFaction.Influence, Is.EqualTo(minorFactionInfo2.Influence));
                    Assert.That(newSystemMinorFaction.State.Select(state => state.Name), Is.EquivalentTo(minorFactionInfo2.States));
                }
                finally
                {
                    if (newSystemMinorFaction != null)
                    {
                        dbContext.SystemMinorFactions.Remove(newSystemMinorFaction);
                        dbContext.StarSystems.Remove(dbContext.StarSystems.First(ss => ss.Name == starSystem));
                        dbContext.MinorFactions.Remove(dbContext.MinorFactions.First(mf => mf.Name == minorFaction));
                    }
                    dbContext.SaveChanges();
                }
            }
        }

        // [Test]
        //public void TestExistingSystemMultipleMinorFactions()
        //{
        //    IDbContextFactory<OrderBotDbContext> dbContextFactory = new OrderBotDbContextFactory(useInMemoryDB);
        //    EddnMessageSink messageSink = new EddnMessageSink(dbContextFactory);

        //    string starSystem = "A";
        //    string minorFaction = "B";
        //    MinorFactionInfo oldMinorFactionInfo1 = new MinorFactionInfo("A", 0.2, new string[] { "A", "B" });
        //    MinorFactionInfo oldMinorFactionInfo2 = new MinorFactionInfo("B", 0.5, new string[] { "B", "C" });
        //    MinorFactionInfo newMinorFactionInfo1 = new MinorFactionInfo("B", 0.6, new string[] { "B" });
        //    MinorFactionInfo newMinorFactionInfo2 = new MinorFactionInfo("C", 0.1, new string[] { "D", "E", "F" });
        //    DateTime timestamp1 = DateTime.UtcNow.AddSeconds(-1).ToUniversalTime();
        //    DateTime timestamp2 = DateTime.UtcNow.ToUniversalTime();
        //    messageSink.Sink(timestamp1, starSystem, new MinorFactionInfo[]
        //    {
        //        oldMinorFactionInfo1,
        //        oldMinorFactionInfo2
        //    });
        //    messageSink.Sink(timestamp2, starSystem, new MinorFactionInfo[]
        //    {
        //        newMinorFactionInfo1,
        //        newMinorFactionInfo2
        //    });

        //    using (OrderBotDbContext dbContext = dbContextFactory.CreateDbContext())
        //    {
        //        IEnumerable<SystemMinorFaction> systemMinorFactions = dbContext.SystemMinorFaction.Include(smf => smf.States);
        //        SystemMinorFaction? newSystemMinorFaction = null;
        //        try
        //        {
        //            Assert.That(systemMinorFactions.Count, Is.EqualTo(1));
        //            newSystemMinorFaction = systemMinorFactions.First();
        //            Assert.That(newSystemMinorFaction.StarSystem, Is.EqualTo(starSystem));
        //            Assert.That(newSystemMinorFaction.MinorFaction, Is.EqualTo(minorFaction));
        //            Assert.That(newSystemMinorFaction.Influence, Is.EqualTo(minorFactionInfo2.Influence));
        //            Assert.That(newSystemMinorFaction.States.Select(smfs => smfs.State), Is.EquivalentTo(minorFactionInfo2.States));
        //            Assert.That(newSystemMinorFaction.LastUpdated, Is.EqualTo(timestamp2).Using(DbDateTimeComparer.Instance));
        //        }
        //        finally
        //        {
        //            if (newSystemMinorFaction != null)
        //            {
        //                dbContext.SystemMinorFaction.Remove(newSystemMinorFaction);
        //            }
        //            dbContext.SaveChanges();
        //        }
        //    }
        //}
    }
}
