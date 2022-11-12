namespace OrderBot.Rbac;

internal record Role
{
    /// <summary>
    /// Create a new <see cref="Role"/>.
    /// </summary>
    /// <param name="name">
    /// The unique name. This appears in the database and similar places.
    /// </param>
    /// <param name="description">
    /// A human-readable description.
    /// </param>
    protected Role(string name, string description)
    {
        Name = name;
        Description = description;
    }

    /// <summary>
    /// The role's unique name, as stored in the datbase.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// A human-readable description.
    /// </summary>
    public string Description { get; }
}
