using OrderBot.Core;
using System.Reactive.Linq;

namespace OrderBot.ToDo
{
    /// <summary>
    /// Base class for goals. These describe the intent or aim for a <see cref="MinorFaction"/> 
    /// in a <see cref="StarSystem"/>. This is communicated via <see cref="Suggestion"/>s of
    /// various types, which appear on the <see cref="ToDoList"/>.
    /// </summary>
    /// <remarks>
    /// Subclasses should be stateless and contain a static property Instance, effectively
    /// a singleton.
    /// </remarks>
    internal abstract class Goal
    {
        /// <summary>
        /// Create a new <see cref="Goal"/>.
        /// </summary>
        /// <param name="name">
        /// The unique name. This appears in the database and similar places.
        /// </param>
        /// <param name="description">
        /// A human-readable description.
        /// </param>
        protected Goal(string name, string description)
        {
            Name = name;
            Description = description;
        }

        /// <summary>
        /// The goal's unique name, as stored in the datbase.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// A human-readable description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Add suggestions to the for the <paramref name="starSystemMinorFaction"/> to the <paramref name="toDoList"/>.
        /// </summary>
        /// <remarks>
        /// Child or subclasses should call 
        /// <see cref="CheckAddActionsPreconditions(Presence, IReadOnlySet{Presence})"/>
        /// first to validate arguements consistently and completely.
        /// </remarks>
        /// <param name="starSystemMinorFaction">
        /// The star system and minor faction to determine whether there are suggestions to perform.
        /// </param>
        /// <param name="systemBgsData">
        /// BGS details for all minor factions in the star system.
        /// </param>
        /// <param name="systemConflicts">
        /// Conflicts in the star systems..
        /// </param>
        /// <param name="toDoList">
        /// Receives suggestions.
        /// </param>
        /// <exception cref="ArgumentException">
        /// <paramref name="systemBgsData"/> must contain <paramref name="starSystemMinorFaction"/>. 
        /// <paramref name="systemBgsData"/> must be for a single star system.
        /// </exception>
        public abstract void AddSuggestions(Presence starSystemMinorFaction,
            IReadOnlySet<Presence> systemBgsData, IReadOnlySet<Conflict> systemConflicts, ToDoList toDoList);

        /// <summary>
        /// Return the controlling minor faction, i.e. the one with the highest influence.
        /// </summary>
        /// <param name="systemBgsData">
        /// The minor factions in a system. This cannot be empty.
        /// </param>
        /// <returns>
        /// The minor faction with the highest influence.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="systemBgsData"/> is empty.
        /// </exception>
        protected internal static Presence GetControllingMinorFaction(IReadOnlySet<Presence> systemBgsData)
        {
            return systemBgsData.OrderByDescending(ssmf => ssmf.Influence)
                                .First();
        }

        /// <summary>
        /// Check the preconditions for 
        /// <see cref="AddSuggestions(Presence, IReadOnlySet{Presence}, ToDoList)'"/>.
        /// </summary>
        /// <param name="starSystemMinorFaction">
        /// Passed in.
        /// </param>
        /// <param name="systemBgsData">
        /// Passed in.
        /// </param>
        /// <param name="systemConflicts">
        /// Passed in.
        /// </param>
        /// <exception cref="ArgumentException">
        /// One or more of:
        /// <list type="bullet">
        /// <item><paramref name="systemBgsData"/> must contain <paramref name="starSystemMinorFaction"/></item>
        /// <item>All <paramref name="systemBgsData"/> must be for the star system in <paramref name="starSystemMinorFaction"/>.</item>
        /// <item><paramref name="systemBgsData"/> must be for a single star system.</item>
        /// <item>All <paramref name="systemConflicts"/> must be in the star system in <paramref name="starSystemMinorFaction"/>.</item>
        /// <item>All minor factions in <paramref name="systemConflicts"/> must be in <paramref name="systemBgsData"/>.</item>
        /// </list>
        /// </exception>
        protected internal static void CheckAddActionsPreconditions(Presence starSystemMinorFaction,
            IReadOnlySet<Presence> systemBgsData, IReadOnlySet<Conflict> systemConflicts)
        {
            if (systemBgsData.Any(ssmf => ssmf.StarSystem != starSystemMinorFaction.StarSystem))
            {
                throw new ArgumentException($"All {nameof(systemBgsData)} must be for star system {starSystemMinorFaction.StarSystem.Name}");
            }
            if (!systemBgsData.Contains(starSystemMinorFaction))
            {
                throw new ArgumentException($"{nameof(systemBgsData)} must contain {nameof(starSystemMinorFaction)}");
            }
            if (systemConflicts.Any(c => c.StarSystem != starSystemMinorFaction.StarSystem))
            {
                throw new ArgumentException($"All {nameof(systemConflicts)} must be in star system {starSystemMinorFaction.StarSystem.Name}");
            }
            if (!systemBgsData.Select(ssmf => ssmf.MinorFaction)
                             .ToHashSet()
                             .IsSupersetOf(systemConflicts.SelectMany(c => new MinorFaction[] { c.MinorFaction1, c.MinorFaction2 })))
            {
                throw new ArgumentException($"All minor factions in {nameof(systemConflicts)} must be in {nameof(systemBgsData)}");
            }
        }

        /// <summary>
        /// Add the first <see cref="ConflictSuggestion"/>s to <see cref="ToDoList"/>. A minor faction
        /// can only participate in one conflict at a time.
        /// </summary>
        /// <param name="systemConflicts">
        /// All conflicts in this system.
        /// </param>
        /// <param name="toDoList">
        /// The <see cref="ToDoList"/> to add the goals to.
        /// </param>
        /// <param name="getConflicts">
        /// Call each of these, in order, on the <paramref name="systemConflicts"/> to determine
        /// if we should fight in this war. Return the first non-null <see cref="ConflictSuggestion"/>.
        /// See <see cref="Fight"/> for options.
        /// </param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        protected internal static bool AddConflicts(IReadOnlySet<Conflict> systemConflicts, ToDoList toDoList,
            params Func<Conflict, ConflictSuggestion?>[] getConflicts)
        {
            bool conflictAdded = false;

            ConflictSuggestion? conflictSuggestion = getConflicts.Select(gc => systemConflicts.Select(c => gc(c))
                                                                                              .FirstOrDefault(cs => cs != null))
                                                                 .FirstOrDefault(cs => cs != null);
            if (conflictSuggestion != null)
            {
                if (Conflict.IsWar(conflictSuggestion.WarType))
                {
                    toDoList.Wars.Add(conflictSuggestion);
                }
                else if (Conflict.IsElection(conflictSuggestion.WarType))
                {
                    toDoList.Elections.Add(conflictSuggestion);
                }
                else
                {
                    // Defensive
                    throw new InvalidOperationException($"Unknown war type in {conflictSuggestion}");
                }

                conflictAdded = true;
            }

            return conflictAdded;
        }
    }
}
