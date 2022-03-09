using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OrderBot.Core;

namespace EddnMessageProcessor
{
    internal class EddnMessageSink
    {
        public EddnMessageSink(IDbContextFactory<OrderBotDbContext> dbContextFactory)
        {
            DbContextFactory = dbContextFactory;
        }

        public IDbContextFactory<OrderBotDbContext> DbContextFactory { get; }

        public void Sink(DateTime timestamp, string starSystemName, IEnumerable<MinorFactionInfo> minorFactionDetails)
        {
            using (OrderBotDbContext dbContext = DbContextFactory.CreateDbContext())
            using (TransactionScope transactionScope = new TransactionScope())
            {
                StarSystem? starSystem = dbContext.StarSystems.FirstOrDefault(starSystem => starSystem.Name == starSystemName);
                if(starSystem == null)
                {
                    starSystem = new StarSystem { Name = starSystemName, LastUpdated = timestamp };
                    dbContext.StarSystems.Add(starSystem);
                }
                else
                {
                    starSystem.LastUpdated = timestamp;
                }

                // Add or update minor factions
                foreach (MinorFactionInfo newMinorFactionInfo in minorFactionDetails)
                {
                    MinorFaction? minorFaction = dbContext.MinorFactions.FirstOrDefault(minorFaction => minorFaction.Name == newMinorFactionInfo.MinorFaction);
                    if (minorFaction == null)
                    {
                        minorFaction = new MinorFaction { Name = newMinorFactionInfo.MinorFaction };
                        dbContext.MinorFactions.Add(minorFaction);
                    }

                    StarSystemMinorFaction? dbSystemMinorFaction = dbContext.StarSystemMinorFactions
                                                                            .Include(smf => smf.States)
                                                                            .Include(smf => smf.StarSystem)
                                                                            .Include(smf => smf.MinorFaction)
                                                                            .FirstOrDefault(
                                                                                smf => smf.StarSystem == starSystem 
                                                                                && smf.MinorFaction == minorFaction)                                                                        ;
                    if (dbSystemMinorFaction == null)
                    {
                        dbSystemMinorFaction = new StarSystemMinorFaction
                        {
                            MinorFaction = minorFaction,
                            StarSystem = starSystem,
                            Influence = newMinorFactionInfo.Influence,
                        };
                        dbContext.StarSystemMinorFactions.Add(dbSystemMinorFaction);
                    }
                    else
                    {
                        dbSystemMinorFaction.Influence = newMinorFactionInfo.Influence;
                    }

                    dbSystemMinorFaction.States.Clear();
                    foreach (string stateName in newMinorFactionInfo.States)
                    {
                        State? state = dbContext.States.FirstOrDefault(s => s.Name == stateName);
                        if (state == null)
                        {
                            state = new State { Name = stateName };
                            dbContext.States.Add(state);
                        }
                        dbSystemMinorFaction.States.Add(state);
                    }

                    dbContext.SaveChanges();
                }

                // Delete old minor factions
                SortedSet<string> newSystemMinorFactions = new SortedSet<string>(minorFactionDetails.Select(mfd => mfd.MinorFaction));
                foreach (StarSystemMinorFaction systemMinorFaction in dbContext.StarSystemMinorFactions
                                                                               .Where(smf => smf.StarSystem == starSystem
                                                                                    && !newSystemMinorFactions.Contains(smf.MinorFaction.Name)))
                {
                    dbContext.StarSystemMinorFactions.Remove(systemMinorFaction);
                }

                dbContext.SaveChanges();
                transactionScope.Complete();
            }
        }
    }
}
