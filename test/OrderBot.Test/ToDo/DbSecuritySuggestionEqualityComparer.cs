using OrderBot.ToDo;
using System.Diagnostics.CodeAnalysis;

namespace OrderBot.Test.ToDo;

/// <summary>
/// Compare two <see cref="SecuritySuggestion"/>s, ignoring database-induced 
/// differences with <see cref="DateTime"/>s.
/// </summary>
internal class DbSecuritySuggestionEqualityComparer : IEqualityComparer<SecuritySuggestion>
{
    public static readonly DbSecuritySuggestionEqualityComparer Instance = new();

    /// <summary>
    /// Prevent instantiation.
    /// </summary>
    protected DbSecuritySuggestionEqualityComparer()
    {
        // Do nothing
    }

    public bool Equals(SecuritySuggestion? x, SecuritySuggestion? y)
    {
        return x is not null &&
               y is not null &&
               x.StarSystem.Id == y.StarSystem.Id &&
               x.StarSystem.Name == y.StarSystem.Name &&
               DbDateTimeComparer.Instance.Equals(x.StarSystem.LastUpdated, y.StarSystem.LastUpdated) &&
               x.SecurityLevel == y.SecurityLevel;
    }

    public int GetHashCode([DisallowNull] SecuritySuggestion obj)
    {
        throw new NotImplementedException();
    }
}
