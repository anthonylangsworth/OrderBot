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

        public void Sink(DateTime timestamp, string starSystemName, IEnumerable<MinorFactionInfo> minorFactionDetails)
        {
            using (OrderBotDbContext dbContext = DbContextFactory.CreateDbContext())
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

                    StarSystemMinorFaction? dbSystemMinorFaction = dbContext.SystemMinorFactions
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
                        dbContext.SystemMinorFactions.Add(dbSystemMinorFaction);
                    }
                    else
                    {
                        dbSystemMinorFaction.Influence = newMinorFactionInfo.Influence;
                    }

                    dbSystemMinorFaction.States.Clear();
                    dbSystemMinorFaction.States.AddRange(newMinorFactionInfo.States.Select(state => new State { Name = state }));
                }

                // Delete old minor factions
                SortedSet<string> newSystemMinorFactions = new SortedSet<string>(minorFactionDetails.Select(mfd => mfd.MinorFaction));
                foreach (StarSystemMinorFaction systemMinorFaction in dbContext.SystemMinorFactions
                                                                               .Where(smf => !newSystemMinorFactions.Contains(smf.MinorFaction.Name)))
                {
                    dbContext.SystemMinorFactions.Remove(systemMinorFaction);
                }

                dbContext.SaveChanges();
            }
        }
    }
}
