using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderBot.Core.Test
{
    /// <summary>
    /// SQL Server's datetime type has a resolution to ms. The .Net DateTime type
    /// has a higher resolution. This IEqualityComparer compares two DateTime
    /// objects down to the millisecond.
    /// </summary>
    public class DbDateTimeComparer : IEqualityComparer<DateTime>
    {
        private static DbDateTimeComparer _instance = new DbDateTimeComparer();

        public bool Equals(DateTime x, DateTime y)
        {
            return (x - y).TotalMilliseconds < 1000;
        }

        public int GetHashCode([DisallowNull] DateTime obj)
        {
            throw new NotImplementedException();
        }

        public static DbDateTimeComparer Instance => _instance;
    }
}
