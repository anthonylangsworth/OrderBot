using OrderBot.Core;

namespace OrderBot.ToDo
{
    internal abstract class Goal
    {
        protected Goal(string name, string description)
        {
            Name = name;
            Description = description;
        }

        /// <summary>
        /// The goal's name, as stored in the datbase.
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
        /// <see cref="CheckAddActionsPreconditions(StarSystemMinorFaction, IReadOnlySet{StarSystemMinorFaction})"/>
        /// first to validate arguements consistently and completely.
        /// </remarks>
        /// <param name="starSystemMinorFaction">
        /// The star system and minor faction to determine whether there are suggestions to perform.
        /// </param>
        /// <param name="systemBgsData">
        /// BGS details for all minor factions in the star system.
        /// </param>
        /// <param name="conflicts">
        /// Conflicts in the star systems..
        /// </param>
        /// <param name="toDoList">
        /// Receives suggestions.
        /// </param>
        /// <exception cref="ArgumentException">
        /// <paramref name="systemBgsData"/> must contain <paramref name="starSystemMinorFaction"/>. 
        /// <paramref name="systemBgsData"/> must be for a single star system.
        /// </exception>
        public abstract void AddSuggestions(StarSystemMinorFaction starSystemMinorFaction,
            IReadOnlySet<StarSystemMinorFaction> systemBgsData, IReadOnlySet<Conflict> conflicts, ToDoList toDoList);

        /// <summary>
        /// Return the controlling minor faction, i.e. the one with the highest influence.
        /// </summary>
        /// <param name="systemBgsData">
        /// The minor factions in a system.
        /// </param>
        /// <returns>
        /// The minor faction with the highest influence.
        /// </returns>
        protected internal static StarSystemMinorFaction GetControllingMinorFaction(IReadOnlySet<StarSystemMinorFaction> systemBgsData)
        {
            return systemBgsData.OrderByDescending(ssmf => ssmf.Influence)
                                .First();
        }

        /// <summary>
        /// Check the preconditions for 
        /// <see cref="AddSuggestions(StarSystemMinorFaction, IReadOnlySet{StarSystemMinorFaction}, ToDoList)'"/>.
        /// </summary>
        /// <param name="starSystemMinorFaction">
        /// Passed in.
        /// </param>
        /// <param name="systemBgsData">
        /// Passed in.
        /// </param>
        /// <param name="conflicts">
        /// Passed in.
        /// </param>
        /// <exception cref="ArgumentException">
        /// <paramref name="systemBgsData"/> must contain <paramref name="starSystemMinorFaction"/>. 
        /// <paramref name="systemBgsData"/> must be for a single star system.
        /// </exception>
        protected internal static void CheckAddActionsPreconditions(StarSystemMinorFaction starSystemMinorFaction,
            IReadOnlySet<StarSystemMinorFaction> systemBgsData, IReadOnlySet<Conflict> conflicts)
        {
            if (systemBgsData.Any(ssmf => ssmf.StarSystem != starSystemMinorFaction.StarSystem))
            {
                throw new ArgumentException($"All {nameof(systemBgsData)} must be for star system {starSystemMinorFaction.StarSystem.Name}");
            }
            if (!systemBgsData.Contains(starSystemMinorFaction))
            {
                throw new ArgumentException($"{nameof(systemBgsData)} must contain {nameof(starSystemMinorFaction)}");
            }
            if (conflicts.Any(c => c.StarSystem != starSystemMinorFaction.StarSystem))
            {
                throw new ArgumentException($"All {nameof(conflicts)} must be for star system {starSystemMinorFaction.StarSystem.Name}");
            }
        }
    }
}
