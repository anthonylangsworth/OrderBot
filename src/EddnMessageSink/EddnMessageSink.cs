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

        public void Sink(DateTime timestamp, string starSystem, MinorFactionInfo[] minorFactionDetails)
        {
            using (OrderBotDbContext dbContext = DbContextFactory.CreateDbContext())
            {
                foreach (MinorFactionInfo newMinorFactionInfo in minorFactionDetails)
                {
                    SystemMinorFaction? existingSystemMinorFaction = dbContext.SystemMinorFaction
                                                                              .FirstOrDefault(smf => smf.StarSystem == starSystem && smf.MinorFaction == newMinorFactionInfo.MinorFaction);
                    if (existingSystemMinorFaction == null)
                    {
                        existingSystemMinorFaction = new SystemMinorFaction
                        {
                            MinorFaction = newMinorFactionInfo.MinorFaction,
                            StarSystem = starSystem,
                            Influence = newMinorFactionInfo.Influence,
                            LastUpdated = DateTime.UtcNow
                        };
                        dbContext.SystemMinorFaction.Add(existingSystemMinorFaction);
                    }
                    else
                    {
                        existingSystemMinorFaction.Influence = newMinorFactionInfo.Influence;
                        existingSystemMinorFaction.LastUpdated = DateTime.UtcNow;
                    }

                    existingSystemMinorFaction.States.Clear();
                    existingSystemMinorFaction.States.AddRange(newMinorFactionInfo.States.Select(state => new SystemMinorFactionState { State = state }));
                }

                dbContext.SaveChanges();
            }
        }
    }
}
