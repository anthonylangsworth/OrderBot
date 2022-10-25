using OrderBot.ToDo;
using System.Diagnostics.CodeAnalysis;

namespace OrderBot.Test.ToDo
{
    internal class MinorFactionInfluenceEqualityComparer : IEqualityComparer<MinorFactionInfluence>
    {
        public static readonly MinorFactionInfluenceEqualityComparer Instance = new();

        /// <summary>
        /// Prevent instantiation.
        /// </summary>
        protected MinorFactionInfluenceEqualityComparer()
        {
            // Do nothing
        }

        public bool Equals(MinorFactionInfluence? x, MinorFactionInfluence? y)
        {
            return x.MinorFaction == y.MinorFaction
                && x.Influence == y.Influence
                && x.States.OrderBy(s => s).SequenceEqual(y.States.OrderBy(s => s));
        }

        public int GetHashCode([DisallowNull] MinorFactionInfluence obj)
        {
            throw new NotImplementedException();
        }
    }
}
