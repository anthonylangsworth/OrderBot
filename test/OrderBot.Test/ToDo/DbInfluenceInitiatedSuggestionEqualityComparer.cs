using OrderBot.ToDo;
using System.Diagnostics.CodeAnalysis;

namespace OrderBot.Test.ToDo
{
    /// <summary>
    /// Compare two <see cref="InfluenceSuggestion"/>s, ignoring database-induced 
    /// differences with <see cref="DateTime"/>s.
    /// </summary>
    internal class DbInfluenceInitiatedSuggestionEqualityComparer : IEqualityComparer<InfluenceSuggestion>
    {
        public static readonly DbInfluenceInitiatedSuggestionEqualityComparer Instance = new();

        /// <summary>
        /// Prevent instantiation.
        /// </summary>
        protected DbInfluenceInitiatedSuggestionEqualityComparer()
        {
            // Do nothing
        }

        public bool Equals(InfluenceSuggestion? x, InfluenceSuggestion? y)
        {
            return x is not null &&
                   y is not null &&
                   x.StarSystem.Id == y.StarSystem.Id &&
                   x.StarSystem.Name == y.StarSystem.Name &&
                   DbDateTimeComparer.Instance.Equals(x.StarSystem.LastUpdated, y.StarSystem.LastUpdated) &&
                   x.Influence == y.Influence;
        }

        public int GetHashCode([DisallowNull] InfluenceSuggestion obj)
        {
            throw new NotImplementedException();
        }
    }
}
