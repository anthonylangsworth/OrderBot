using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrderBot.Core;

namespace EddnMessageProcessor
{
    internal class EddnMessageSink
    {
        public void Sink(DateTime timestamp, string? starSystem, MinorFactionInfo[] minorFactionDetails)
        {
            using (OrderBotDbContext dbContext = new OrderBotDbContext())
            {
                foreach (MinorFactionInfo newMinorFactionInfo in minorFactionDetails)
                {
                    SystemMinorFaction? existingSystemMinorFaction = dbContext.SystemMinorFaction
                                                                              .FirstOrDefault(smf => smf.StarSystem == starSystem && smf.MinorFaction == newMinorFactionInfo.minorFaction);
                    if (existingSystemMinorFaction == null)
                    {
                        existingSystemMinorFaction = new SystemMinorFaction
                        {
                            MinorFaction = newMinorFactionInfo.minorFaction,
                            StarSystem = starSystem,
                            Influence = newMinorFactionInfo.influence,
                            LastUpdated = DateTime.UtcNow
                        };
                        dbContext.SystemMinorFaction.Add(existingSystemMinorFaction);
                    }
                    else
                    {
                        existingSystemMinorFaction.Influence = newMinorFactionInfo.influence;
                        existingSystemMinorFaction.LastUpdated = DateTime.UtcNow;
                    }

                    existingSystemMinorFaction.States.Clear();
                    existingSystemMinorFaction.States.AddRange(newMinorFactionInfo.states);
                }

                dbContext.SaveChanges();
            }
        }
    }
}
