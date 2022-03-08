using OrderBot.Core;
using OrderBot.Core.Test;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddnMessageProcessor.Test
{
    public static class Helpers
    {
        public static bool IsSame(StarSystemMinorFaction starSystemMinorFaction, string starSystemName, DateTime lastUpdated,
            MinorFactionInfo minorFactionInfo)
        {
            return starSystemMinorFaction.StarSystem != null
                && starSystemMinorFaction.StarSystem.Name == starSystemName
                && DbDateTimeComparer.Instance.Equals(starSystemMinorFaction.StarSystem.LastUpdated, lastUpdated)
                && starSystemMinorFaction.Influence == minorFactionInfo.Influence
                && starSystemMinorFaction.MinorFaction != null
                && starSystemMinorFaction.MinorFaction.Name == minorFactionInfo.MinorFaction
                && starSystemMinorFaction.States.Select(x => x.Name).OrderBy(x => x).SequenceEqual(minorFactionInfo.States.OrderBy(x => x));
        }
    }
}
