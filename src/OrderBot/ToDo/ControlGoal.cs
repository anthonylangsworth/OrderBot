using OrderBot.Core;

namespace OrderBot.ToDo
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
        public override void AddSuggestions(StarSystemMinorFaction starSystemMinorFaction,
            IReadOnlySet<StarSystemMinorFaction> systemBgsData, IReadOnlySet<Conflict> systemConflicts, ToDoList toDoList)
        {
            CheckAddActionsPreconditions(starSystemMinorFaction, systemBgsData, systemConflicts);

            bool conflictAdded = false;

            // Technically, a minor faction can only be in one conflict at a time.
            foreach (Conflict conflict in systemConflicts.Where(c => c.MinorFaction1 == starSystemMinorFaction.MinorFaction
                                                                  || c.MinorFaction2 == starSystemMinorFaction.MinorFaction))
            {
                MinorFaction fightFor = null!;
                int fightForWonDays;
                MinorFaction fightAgainst = null!;
                int fightAgainstWonDays;

                if (conflict.MinorFaction1 == starSystemMinorFaction.MinorFaction)
                {
                    fightFor = conflict.MinorFaction1;
                    fightForWonDays = conflict.MinorFaction1WonDays;
                    fightAgainst = conflict.MinorFaction2;
                    fightAgainstWonDays = conflict.MinorFaction2WonDays;
                }
                else if (conflict.MinorFaction2 == starSystemMinorFaction.MinorFaction)
                {
                    fightFor = conflict.MinorFaction2;
                    fightForWonDays = conflict.MinorFaction2WonDays;
                    fightAgainst = conflict.MinorFaction1;
                    fightAgainstWonDays = conflict.MinorFaction1WonDays;
                }
                else
                {
                    // Defensive
                    throw new InvalidOperationException($"Conflict with unknown minor faction");
                }

                ConflictSuggestion conflictSuggestion = new()
                {
                    StarSystem = starSystemMinorFaction.StarSystem,
                    FightFor = fightFor,
                    FightForWonDays = fightForWonDays,
                    FightAgainst = fightAgainst,
                    FightAgainstWonDays = fightAgainstWonDays,
                    State = Conflict.GetState(conflict.Status, fightForWonDays, fightAgainstWonDays)
                };
                if (Conflict.IsWar(conflict.WarType))
                {
                    toDoList.Wars.Add(conflictSuggestion);
                }
                else if (Conflict.IsElection(conflict.WarType))
                {
                    toDoList.Elections.Add(conflictSuggestion);
                }
                else
                {
                    // Defensive
                    throw new InvalidOperationException($"Unknown war type {conflict.WarType}");
                }

                conflictAdded = true;
            }

            if (!conflictAdded)
            {
                if (starSystemMinorFaction.Influence < LowerInfluenceThreshold)
                {
                    toDoList.Pro.Add(new InfluenceSuggestion
                    {
                        StarSystem = starSystemMinorFaction.StarSystem,
                        Influence = starSystemMinorFaction.Influence
                    });
                }
                else if (starSystemMinorFaction.Influence > UpperInfluenceThreshold)
                {
                    toDoList.Anti.Add(new InfluenceSuggestion { StarSystem = starSystemMinorFaction.StarSystem, Influence = starSystemMinorFaction.Influence });
                }
            }

            // Security only applies for the controlling minor faction
            if (starSystemMinorFaction == GetControllingMinorFaction(systemBgsData)
                && starSystemMinorFaction.SecurityLevel == SecurityLevel.Low)
            {
                toDoList.ProSecurity.Add(new SecuritySuggestion { StarSystem = starSystemMinorFaction.StarSystem, SecurityLevel = starSystemMinorFaction.SecurityLevel });
            }
        }
    }
}
