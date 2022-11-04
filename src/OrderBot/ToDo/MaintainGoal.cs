﻿using OrderBot.Core;

namespace OrderBot.ToDo
{
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
            : base("Maintain", "Maintain presence in the system but do not control it.")
        {
            // Do nothing
        }

        /// <summary>
        /// Do Pro work if the influence falls below this value.
        /// </summary>
        public static double LowerInfluenceThreshold => 0.1;

        /// <summary>
        /// Do Anti work if the influence is within this of the controlling
        /// minor faction's influence.
        /// </summary>
        public static double MaxInfuenceGap => 0.03;

        /// </inheritdoc>
        public override void AddSuggestions(StarSystemMinorFaction starSystemMinorFaction,
            IReadOnlySet<StarSystemMinorFaction> systemBgsData, IReadOnlySet<Conflict> systemConflicts,
            ToDoList toDoList)
        {
            CheckAddActionsPreconditions(starSystemMinorFaction, systemBgsData, systemConflicts);

            if (systemBgsData.Count > 1)
            {
                StarSystemMinorFaction controllingMinorFaction = GetControllingMinorFaction(systemBgsData);

                if (!AddConflicts(systemConflicts, toDoList,
                    c => Fight(controllingMinorFaction.MinorFaction, starSystemMinorFaction.MinorFaction, c),
                    c => FightForOrAgainst(starSystemMinorFaction.MinorFaction, true, c)))
                {
                    double maxInfluence = controllingMinorFaction.Influence - MaxInfuenceGap;
                    if (starSystemMinorFaction.Influence < LowerInfluenceThreshold)
                    {
                        toDoList.Pro.Add(new InfluenceSuggestion
                        {
                            StarSystem = starSystemMinorFaction.StarSystem,
                            Influence = starSystemMinorFaction.Influence
                        });
                    }
                    else if (starSystemMinorFaction.Influence > maxInfluence)
                    {
                        toDoList.Anti.Add(new InfluenceSuggestion
                        {
                            StarSystem = starSystemMinorFaction.StarSystem,
                            Influence = starSystemMinorFaction.Influence
                        });
                    }
                }
            }
        }
    }
}
