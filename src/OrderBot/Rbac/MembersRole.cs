namespace OrderBot.Rbac;

/// <summary>
/// Squadron members, that can view the todo list and some settings.
/// </summary>
internal record MembersRole : Role
{
    /// <summary>
    /// Singleton.
    /// </summary>
    public static MembersRole Instance => new();

    /// <summary>
    /// Prevent instantiation.
    /// </summary>
    private MembersRole()
        : base(RoleName, "Squadron members can view the todo list and some settings")
    {
        // Do nothing
    }

    public const string RoleName = "Members";
}
