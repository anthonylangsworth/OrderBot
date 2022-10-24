using System.Diagnostics.CodeAnalysis;

namespace OrderBot.Test
{
    /// <summary>
    /// SQL Server's datetime type has a resolution to ms. The .Net DateTime type
    /// has a higher resolution. This IEqualityComparer compares two DateTime
    /// objects down to the millisecond.
    /// </summary>
    public class DbDateTimeComparer : IEqualityComparer<DateTime?>
    {
        public static readonly DbDateTimeComparer Instance = new();

        /// <summary>
        /// Prevent instantiation
        /// </summary>
        protected DbDateTimeComparer()
        {
            // Do nothing
        }

        public bool Equals(DateTime? x, DateTime? y)
        {
            if (x == null && y == null)
            {
                return true;
            }
            else if (x != null && y != null)
            {
                return (x - y).Value.TotalMilliseconds < 1000;
            }
            else
            {
                return false;
            }
        }

        public int GetHashCode([DisallowNull] DateTime? obj)
        {
            throw new NotImplementedException();
        }
    }
}
