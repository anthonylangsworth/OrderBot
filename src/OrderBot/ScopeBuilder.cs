namespace OrderBot;

/// <summary>
/// Passing parameters to <see cref="ILogger{T}.BeginScope"/>.
/// </summary>
internal class ScopeBuilder
{
    private readonly Dictionary<string, object> _scope;

    /// <summary>
    /// Create a new <see cref="ScopeBuilder"/>.
    /// </summary>
    public ScopeBuilder()
    {
        _scope = new Dictionary<string, object>();
    }

    /// <summary>
    /// Add new parameter.
    /// </summary>
    /// <param name="name">
    /// The name. Should should be in Pascal case.
    /// </param>
    /// <param name="value">
    /// The value. Should have a human-readable ToString().
    /// </param>
    /// <returns>
    /// This object for fluent use.
    /// </returns>
    public ScopeBuilder Add(string name, object value)
    {
        _scope[name] = value;
        return this;
    }

    /// <summary>
    /// Construct the parameters to pass to <see cref="ILogger.BeginScope"/>.
    /// </summary>
    /// <returns></returns>
    public IReadOnlyCollection<KeyValuePair<string, object>> Build()
    {
        return _scope;
    }
}
