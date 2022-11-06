﻿using OrderBot.Core;

namespace OrderBot.ToDo
{
    /// <summary>
    /// Retreat the minor faction by lowering its influence.
    /// </summary>
    internal class RetreatGoal : Goal
    {
        /// <summary>
        /// Singleton.
        /// </summary>
        public static readonly RetreatGoal Instance = new();

        /// <summary>
        /// Prevent instantiation.
        /// </summary>
        private RetreatGoal()
            : base("Retreat", $"Retreat from the system by reducing influence below {Math.Round(InfluenceThreshold * 100, 0)}% and keeping it there.")
        {
            // Do nothing
        }

        /// <summary>
        /// The influence threshold to force a retreat.
        /// </summary>
        public static double InfluenceThreshold => 0.05;

        /// <inheritdoc/>
        public override void AddSuggestions(Presence starSystemMinorFaction,
            IReadOnlySet<Presence> systemPresences, IReadOnlySet<Conflict> systemConflicts,
            ToDoList toDoList)
        {
            CheckAddActionsPreconditions(starSystemMinorFaction, systemPresences, systemConflicts);

            if (!AddConflicts(systemConflicts, toDoList,
                c => Fight.Against(starSystemMinorFaction.MinorFaction, c)))
            {
                if (starSystemMinorFaction.Influence >= InfluenceThreshold)
                {
                    toDoList.Anti.Add(new() { StarSystem = starSystemMinorFaction.StarSystem, Influence = starSystemMinorFaction.Influence });
                }
            }
        }
    }
}
