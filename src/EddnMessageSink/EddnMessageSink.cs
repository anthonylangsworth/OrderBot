using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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

        public void Sink(DateTime timestamp, string starSystem, IEnumerable<MinorFactionInfo> minorFactionDetails)
        {
            using (OrderBotDbContext dbContext = DbContextFactory.CreateDbContext())
            {
                // Add or update minor factions
                foreach (MinorFactionInfo newMinorFactionInfo in minorFactionDetails)
                {
                    SystemMinorFaction? dbSystemMinorFaction = dbContext.SystemMinorFaction
                                                                        .Include(smf => smf.States)
                                                                        .FirstOrDefault(
                                                                            smf => smf.StarSystem == starSystem 
                                                                            && smf.MinorFaction == newMinorFactionInfo.MinorFaction)                                                                        ;
                    if (dbSystemMinorFaction == null)
                    {
                        dbSystemMinorFaction = new SystemMinorFaction
                        {
                            MinorFaction = newMinorFactionInfo.MinorFaction,
                            StarSystem = starSystem,
                            Influence = newMinorFactionInfo.Influence,
                            LastUpdated = timestamp
                        };
                        dbContext.SystemMinorFaction.Add(dbSystemMinorFaction);
                    }
                    else
                    {
                        dbSystemMinorFaction.Influence = newMinorFactionInfo.Influence;
                        dbSystemMinorFaction.LastUpdated = timestamp;
                    }

                    dbSystemMinorFaction.States.Clear();
                    dbSystemMinorFaction.States.AddRange(newMinorFactionInfo.States.Select(state => new SystemMinorFactionState { State = state }));
                }

                // Delete old  minor factions
                SortedSet<string> newSystemMinorFactions = new SortedSet<string>(minorFactionDetails.Select(mfd => mfd.MinorFaction));
                foreach (SystemMinorFaction systemMinorFaction in dbContext.SystemMinorFaction
                                                                           .Where(smf => !newSystemMinorFactions.Contains(smf.MinorFaction)))
                {
                    dbContext.SystemMinorFaction.Remove(systemMinorFaction);
                }

                dbContext.SaveChanges();
            }
        }
    }
}
