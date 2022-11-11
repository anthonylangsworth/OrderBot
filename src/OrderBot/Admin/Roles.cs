namespace OrderBot.Admin
{
    internal static class Roles
    {
        public static IReadOnlyDictionary<string, Role> Map => new Dictionary<string, Role>()
        {
            { MembersRole.Instance.Name, MembersRole.Instance },
            { OfficersRole.Instance.Name, OfficersRole.Instance }
        };
    }
}
