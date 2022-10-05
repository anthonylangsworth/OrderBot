﻿using OrderBot.Core;

namespace OrderBot.Reports
{
    internal class ControlGoal : Goal
    {
        /// <summary>
        /// Singleton.
        /// </summary>
        public static readonly ControlGoal Instance = new();

        /// <summary>
        /// Prevent instantiation.
        /// </summary>
        private ControlGoal()
            : base("Control", $"Be the minor faction with the highest influence. Keep influence between {Math.Round(LowerInfluenceThreshold * 100, 0)}% and {Math.Round(UpperInfluenceThreshold * 100, 0)}%.")
        {
            // Do nothing
        }

        /// <summary>
        /// Work for this minor faction if the influence drops below this level.
        /// </summary>
        public static double LowerInfluenceThreshold => 0.55;

        /// <summary>
        /// Work against this minor faction if the influence raises above this level.
        /// </summary>
        public static double UpperInfluenceThreshold => 0.65;

        /// <inheritdoc/>
        public override void AddActions(StarSystemMinorFaction starSystemMinorFaction, ToDoList toDoList)
        {
            if (starSystemMinorFaction.Influence < LowerInfluenceThreshold)
            {
                toDoList.Pro.Add(new InfluenceInitiatedAction { StarSystem = starSystemMinorFaction.StarSystem, Influence = starSystemMinorFaction.Influence });
            }
            else if (starSystemMinorFaction.Influence > UpperInfluenceThreshold)
            {
                toDoList.Anti.Add(new InfluenceInitiatedAction { StarSystem = starSystemMinorFaction.StarSystem, Influence = starSystemMinorFaction.Influence });
            }

            // TODO: Handle conflicts
        }
    }
}
