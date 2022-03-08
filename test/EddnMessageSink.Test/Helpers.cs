using OrderBot.Core;
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
        public static bool IsSame(StarSystemMinorFaction starSystemMinorFaction, string starSystemName,
            MinorFactionInfo minorFactionInfo)
        {
            return starSystemMinorFaction.StarSystem != null
                && starSystemMinorFaction.StarSystem.Name == starSystemName
                && starSystemMinorFaction.Influence == minorFactionInfo.Influence
                && starSystemMinorFaction.MinorFaction != null
                && starSystemMinorFaction.MinorFaction.Name == minorFactionInfo.MinorFaction
                && starSystemMinorFaction.States.Select(x => x.Name).OrderBy(x => x).SequenceEqual(minorFactionInfo.States.OrderBy(x => x));
        }
    }
}
