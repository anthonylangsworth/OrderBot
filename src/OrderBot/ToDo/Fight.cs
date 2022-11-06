using OrderBot.Core;

namespace OrderBot.ToDo
{
    /// <summary>
    /// Functions to determine whether to participate in a conflict. Usually passed to 
    /// <see cref="Goal.AddConflicts(IReadOnlySet{Conflict}, ToDoList, Func{Conflict, ConflictSuggestion?}[])"/>.
    /// </summary>
    internal static class Fight
    {
        /// <summary>
        /// Participate in a conflict if it is fighting for <paramref name="fightFor"/>
        /// and aginst <paramref name="fightAgainst"/>/, or null, otherwise. 
        /// </summary>
        /// <param name="fightFor">
        /// The <see cref="MinorFaction"/> to fight for.
        /// </param>
        /// <param name="fightAgainst">
        /// The <see cref="MinorFaction"/> to fight against.
        /// </param>
        /// <param name="conflict">
        /// THe <see cref="Conflict"/> to check.
        /// </param>
        /// <param name="description">
        /// A optional, short, human-readable reason why this suggestion exists.
        /// </param>
        /// <returns>
        /// A <see cref="ConflictSuggestion"/> if we should participate, <c>null</c> otherwise.
        /// </returns>
        internal static ConflictSuggestion? Between(MinorFaction fightFor, MinorFaction fightAgainst,
            Conflict conflict, string? description = null)
        {
            MinorFaction fightForMinorFaction = null!;
            int fightForWonDays = 0;
            MinorFaction fightAgainstMinorFaction = null!;
            int fightAgainstWonDays = 0;
            bool noConflict = false;

            if (conflict.MinorFaction1 == fightAgainst && conflict.MinorFaction2 == fightFor)
            {
                fightForMinorFaction = conflict.MinorFaction2;
                fightForWonDays = conflict.MinorFaction2WonDays;
                fightAgainstMinorFaction = conflict.MinorFaction1;
                fightAgainstWonDays = conflict.MinorFaction1WonDays;
            }
            else if (conflict.MinorFaction1 == fightFor && conflict.MinorFaction2 == fightAgainst)
            {
                fightForMinorFaction = conflict.MinorFaction1;
                fightForWonDays = conflict.MinorFaction1WonDays;
                fightAgainstMinorFaction = conflict.MinorFaction2;
                fightAgainstWonDays = conflict.MinorFaction2WonDays;
            }
            else
            {
                noConflict = true;
            }

            return noConflict ? null : new()
            {
                StarSystem = conflict.StarSystem,
                FightFor = fightForMinorFaction,
                FightForWonDays = fightForWonDays,
                FightAgainst = fightAgainstMinorFaction,
                FightAgainstWonDays = fightAgainstWonDays,
                State = Conflict.GetState(conflict.Status, fightForWonDays, fightAgainstWonDays),
                WarType = conflict.WarType,
                Description = description
            };
        }

        /// <summary>
        /// Fight for <paramref name="minorFaction"/>.
        /// </summary>
        /// <param name="minorFaction">
        /// The <see cref="MinorFaction"/> to fight for.
        /// </param>
        /// <param name="conflict">
        /// The <see cref="Conflict"/> to check.
        /// </param>
        /// <param name="description">
        /// A optional, short, human-readable reason why this suggestion exists.
        /// </param>
        /// <returns>
        /// A <see cref="ConflictSuggestion"/> if we should participate, <c>null</c> otherwise.
        /// </returns>
        internal static ConflictSuggestion? For(MinorFaction minorFaction, Conflict conflict,
            string? description = null)
            => ForOrAgainst(minorFaction, true, conflict, description);

        /// <summary>
        /// Fight agsinst <paramref name="minorFaction"/>.
        /// </summary>
        /// <param name="minorFaction">
        /// The <see cref="MinorFaction"/> to fight against.
        /// </param>
        /// <param name="conflict">
        /// The <see cref="Conflict"/> to check.
        /// </param>
        /// <param name="description">
        /// A optional, short, human-readable reason why this suggestion exists.
        /// </param>
        /// <returns>
        /// A <see cref="ConflictSuggestion"/> if we should participate, <c>null</c> otherwise.
        /// </returns>
        internal static ConflictSuggestion? Against(MinorFaction minorFaction, Conflict conflict,
            string? description = null)
            => ForOrAgainst(minorFaction, false, conflict, description);

        /// <summary>
        /// Participate in a conflict if it is fighting for or against <paramref name="minorFaction"/>, 
        /// as determined by <paramref name="fightFor"/>, or null, otherwise. 
        /// </summary>
        /// <param name="minorFaction">
        /// The <see cref="MinorFaction"/> to fight for or against.
        /// </param>
        /// <param name="fightFor">
        /// <c>true</c> if we want to fight for the <paramref name="minorFaction"/>>,
        /// <c>false</c> if we want to fight against it.
        /// </param>
        /// <param name="conflict">
        /// The <see cref="Conflict"/> to check.
        /// </param>
        /// <param name="description">
        /// A optional, short, human-readable reason why this suggestion exists.
        /// </param>
        /// <returns>
        /// A <see cref="ConflictSuggestion"/> if we should participate, <c>null</c> otherwise.
        /// </returns>
        internal static ConflictSuggestion? ForOrAgainst(
            MinorFaction minorFaction, bool fightFor, Conflict conflict, string? description = null)
        {
            MinorFaction fightForMinorFaction = null!;
            int fightForWonDays = 0;
            MinorFaction fightAgainstMinorFaction = null!;
            int fightAgainstWonDays = 0;
            bool noConflict = false;

            if (conflict.MinorFaction1 == minorFaction)
            {
                fightForMinorFaction = fightFor ? conflict.MinorFaction1 : conflict.MinorFaction2;
                fightForWonDays = fightFor ? conflict.MinorFaction1WonDays : conflict.MinorFaction2WonDays;
                fightAgainstMinorFaction = fightFor ? conflict.MinorFaction2 : conflict.MinorFaction1;
                fightAgainstWonDays = fightFor ? conflict.MinorFaction2WonDays : conflict.MinorFaction1WonDays;
            }
            else if (conflict.MinorFaction2 == minorFaction)
            {
                fightForMinorFaction = fightFor ? conflict.MinorFaction2 : conflict.MinorFaction1;
                fightForWonDays = fightFor ? conflict.MinorFaction2WonDays : conflict.MinorFaction1WonDays;
                fightAgainstMinorFaction = fightFor ? conflict.MinorFaction1 : conflict.MinorFaction2;
                fightAgainstWonDays = fightFor ? conflict.MinorFaction1WonDays : conflict.MinorFaction2WonDays;
            }
            else
            {
                noConflict = true;
            }

            return noConflict ? null : new()
            {
                StarSystem = conflict.StarSystem,
                FightFor = fightForMinorFaction,
                FightForWonDays = fightForWonDays,
                FightAgainst = fightAgainstMinorFaction,
                FightAgainstWonDays = fightAgainstWonDays,
                State = Conflict.GetState(conflict.Status, fightForWonDays, fightAgainstWonDays),
                WarType = conflict.WarType,
                Description = description
            };
        }
    }
}