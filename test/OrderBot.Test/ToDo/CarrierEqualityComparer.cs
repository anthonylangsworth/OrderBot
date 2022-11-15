using OrderBot.Core;
using System.Diagnostics.CodeAnalysis;

namespace OrderBot.Test.ToDo;
internal class CarrierEqualityComparer : IEqualityComparer<Carrier>
{
    public static readonly CarrierEqualityComparer Instance = new CarrierEqualityComparer();

    public bool Equals(Carrier? x, Carrier? y)
    {
        // Ignore ID because it is DB assigned
        return x != null
            && y != null
               && x.SerialNumber == y.SerialNumber
               && x.Name == y.Name
               && x.StarSystem == y.StarSystem
               && DbDateTimeComparer.Instance.Equals(x.FirstSeen, y.FirstSeen);
    }

    public int GetHashCode([DisallowNull] Carrier obj)
    {
        throw new NotImplementedException();
    }
}
