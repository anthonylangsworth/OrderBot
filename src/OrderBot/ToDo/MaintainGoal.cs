using OrderBot.Core;

namespace OrderBot.ToDo
{
    /// <summary>
    /// Keep the minor faction in the system but do not control it.
    /// </summary>
    internal class MaintainGoal : Goal
    {
        /// <summary>
        /// Singleton.
        /// </summary>
        public static readonly MaintainGoal Instance = new();

        /// <summary>
        /// Prevent instantiation.
        /// </summary>
        private MaintainGoal()
            : base("Maintain", "Maintain a presence in the system but do not control it.")
        {
            // Do nothing
        }

        /// <summary>
        /// Do Pro work if the influence falls below this value. This is intentionally
        /// higher than <see cref="RetreatGoal.InfluenceThreshold"/>.
        /// </summary>
        public static double LowerInfluenceThreshold => 0.1;

        /// <summary>
        /// Do Anti work if the influence is within this of the controlling
        /// minor faction's influence.
        /// </summary>
        public static double MaxInfuenceGap => 0.03;

        /// </inheritdoc>
        public override void AddSuggestions(Presence starSystemMinorFaction,
            IReadOnlySet<Presence> systemPresences, IReadOnlySet<Conflict> systemConflicts,
            ToDoList toDoList)
        {
            CheckAddActionsPreconditions(starSystemMinorFaction, systemPresences, systemConflicts);

            Presence controllingMinorFaction = GetControllingPresence(systemPresences);
            if (controllingMinorFaction.MinorFaction == starSystemMinorFaction.MinorFaction)
            {
                if (systemPresences.Count > 1)
                {
                    if (!AddConflicts(systemConflicts, toDoList,
                        c => Fight.Against(starSystemMinorFaction.MinorFaction, c, "Avoid Control")))
                    {
                        toDoList.Suggestions.Add(new InfluenceSuggestion
                        {
                            StarSystem = starSystemMinorFaction.StarSystem,
                            Influence = starSystemMinorFaction.Influence,
                            Description = "Avoid Control",
                            Pro = false
                        });
                    }
                }
            }
            else
            {
                if (!AddConflicts(systemConflicts, toDoList,
                    c => Fight.Between(controllingMinorFaction.MinorFaction, starSystemMinorFaction.MinorFaction, c, "Avoid Control"),
                    c => Fight.For(starSystemMinorFaction.MinorFaction, c)))
                {
                    double maxInfluence = controllingMinorFaction.Influence - MaxInfuenceGap;
                    if (starSystemMinorFaction.Influence < LowerInfluenceThreshold)
                    {
                        toDoList.Suggestions.Add(new InfluenceSuggestion
                        {
                            StarSystem = starSystemMinorFaction.StarSystem,
                            Influence = starSystemMinorFaction.Influence,
                            Pro = true
                        });
                    }
                    else if (starSystemMinorFaction.Influence > maxInfluence)
                    {
                        toDoList.Suggestions.Add(new InfluenceSuggestion
                        {
                            StarSystem = starSystemMinorFaction.StarSystem,
                            Influence = starSystemMinorFaction.Influence,
                            Pro = true
                        });
                    }
                }
            }
        }
    }
}
