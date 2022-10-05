using OrderBot.Reports;
using System.Diagnostics.CodeAnalysis;

namespace OrderBot.Test.Reports
{
    internal class DbInfluenceInitiatedActionEqualityComparer : IEqualityComparer<InfluenceInitiatedAction>
    {
        public static readonly DbInfluenceInitiatedActionEqualityComparer Instance = new();

        /// <summary>
        /// Prevent instantiation.
        /// </summary>
        protected DbInfluenceInitiatedActionEqualityComparer()
        {
            // Do nothing
        }

        public bool Equals(InfluenceInitiatedAction? x, InfluenceInitiatedAction? y)
        {
            return x is not null &&
                   y is not null &&
                   x.StarSystem.Id == y.StarSystem.Id &&
                   x.StarSystem.Name == y.StarSystem.Name &&
                   DbDateTimeComparer.Instance.Equals(x.StarSystem.LastUpdated, y.StarSystem.LastUpdated) &&
                   x.Influence == y.Influence;
        }

        public int GetHashCode([DisallowNull] InfluenceInitiatedAction obj)
        {
            throw new NotImplementedException();
        }
    }
}
