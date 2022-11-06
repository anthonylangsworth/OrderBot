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
        /// <param name="systemPresences">
        /// BGS details for all minor factions in the star system.
        /// </param>
        /// <param name="systemConflicts">
        /// Conflicts in the star systems..
        /// </param>
        /// <exception cref="ArgumentException">
        /// <paramref name="systemPresences"/> must contain <paramref name="starSystemMinorFaction"/>. 
        /// <paramref name="systemPresences"/> must be for a single star system.
        /// </exception>
        public abstract IEnumerable<Suggestion> GetSuggestions(Presence starSystemMinorFaction,
            IReadOnlySet<Presence> systemPresences, IReadOnlySet<Conflict> systemConflicts);

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
        protected internal static Presence GetControllingPresence(IReadOnlySet<Presence> systemBgsData)
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
        /// <param name="systemPresences">
        /// Passed in.
        /// </param>
        /// <param name="systemConflicts">
        /// Passed in.
        /// </param>
        /// <exception cref="ArgumentException">
        /// One or more of:
        /// <list type="bullet">
        /// <item><paramref name="systemPresences"/> must contain <paramref name="starSystemMinorFaction"/></item>
        /// <item>All <paramref name="systemPresences"/> must be for the star system in <paramref name="starSystemMinorFaction"/>.</item>
        /// <item><paramref name="systemPresences"/> must be for a single star system.</item>
        /// <item>All <paramref name="systemConflicts"/> must be in the star system in <paramref name="starSystemMinorFaction"/>.</item>
        /// <item>All minor factions in <paramref name="systemConflicts"/> must be in <paramref name="systemPresences"/>.</item>
        /// </list>
        /// </exception>
        protected internal static void CheckAddActionsPreconditions(Presence starSystemMinorFaction,
            IReadOnlySet<Presence> systemPresences, IReadOnlySet<Conflict> systemConflicts)
        {
            if (systemPresences.Any(ssmf => ssmf.StarSystem != starSystemMinorFaction.StarSystem))
            {
                throw new ArgumentException($"All {nameof(systemPresences)} must be for star system {starSystemMinorFaction.StarSystem.Name}");
            }
            if (!systemPresences.Contains(starSystemMinorFaction))
            {
                throw new ArgumentException($"{nameof(systemPresences)} must contain {nameof(starSystemMinorFaction)}");
            }
            if (systemConflicts.Any(c => c.StarSystem != starSystemMinorFaction.StarSystem))
            {
                throw new ArgumentException($"All {nameof(systemConflicts)} must be in star system {starSystemMinorFaction.StarSystem.Name}");
            }
            if (!systemPresences.Select(ssmf => ssmf.MinorFaction)
                             .ToHashSet()
                             .IsSupersetOf(systemConflicts.SelectMany(c => new MinorFaction[] { c.MinorFaction1, c.MinorFaction2 })))
            {
                throw new ArgumentException($"All minor factions in {nameof(systemConflicts)} must be in {nameof(systemPresences)}");
            }
        }

        /// <summary>
        /// Add the first <see cref="ConflictSuggestion"/>s to <see cref="ToDoList"/>. A minor faction
        /// can only participate in one conflict at a time.
        /// </summary>
        /// <param name="systemConflicts">
        /// All conflicts in this system.
        /// </param>
        /// <param name="getConflicts">
        /// Call each of these, in order, on the <paramref name="systemConflicts"/> to determine
        /// if we should fight in this war. Return the first non-null <see cref="ConflictSuggestion"/>.
        /// See <see cref="Fight"/> for options.
        /// </param>
        /// <returns>
        /// The <see cref="ConflictSuggestion"/> to add or <c>null</c>, if none.
        /// </returns>
        /// <exception cref="InvalidOperationException"></exception>
        protected internal static ConflictSuggestion? GetConflict(
            IReadOnlySet<Conflict> systemConflicts,
            params Func<Conflict, ConflictSuggestion?>[] getConflicts)
        {
            return getConflicts.Select(gc => systemConflicts.Select(c => gc(c))
                                                            .FirstOrDefault(cs => cs != null))
                               .FirstOrDefault(cs => cs != null);
        }
    }
}
