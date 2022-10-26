using OrderBot.ToDo;
using System.Diagnostics.CodeAnalysis;

namespace OrderBot.Test.ToDo
{
    /// <summary>
    /// Compare two <see cref="InfluenceInitiatedSuggestion"/>s, ignoring database-induced 
    /// differences with <see cref="DateTime"/>s.
    /// </summary>
    internal class DbInfluenceInitiatedSuggestionEqualityComparer : IEqualityComparer<InfluenceInitiatedSuggestion>
    {
        public static readonly DbInfluenceInitiatedSuggestionEqualityComparer Instance = new();

        /// <summary>
        /// Prevent instantiation.
        /// </summary>
        protected DbInfluenceInitiatedSuggestionEqualityComparer()
        {
            // Do nothing
        }

        public bool Equals(InfluenceInitiatedSuggestion? x, InfluenceInitiatedSuggestion? y)
        {
            return x is not null &&
                   y is not null &&
                   x.StarSystem.Id == y.StarSystem.Id &&
                   x.StarSystem.Name == y.StarSystem.Name &&
                   DbDateTimeComparer.Instance.Equals(x.StarSystem.LastUpdated, y.StarSystem.LastUpdated) &&
                   x.Influence == y.Influence;
        }

        public int GetHashCode([DisallowNull] InfluenceInitiatedSuggestion obj)
        {
            throw new NotImplementedException();
        }
    }
}
