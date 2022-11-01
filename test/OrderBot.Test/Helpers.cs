using OrderBot.Core;
using OrderBot.ToDo;

namespace OrderBot.Test.MessageProcessors
{
    public static class Helpers
    {
        internal static bool IsSame(StarSystemMinorFaction starSystemMinorFaction, string starSystemName, DateTime lastUpdated,
            EddnMinorFactionInfluence minorFactionInfo)
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
