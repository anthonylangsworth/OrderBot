namespace OrderBot.ToDo
{
    internal static class Goals
    {
        public static Goal Default => ControlGoal.Instance;

        public static IDictionary<string, Goal> Map => new Dictionary<string, Goal>
        {
            { ControlGoal.Instance.Name, ControlGoal.Instance },
            { MaintainGoal.Instance.Name, MaintainGoal.Instance },
            { ExpandGoal.Instance.Name, ExpandGoal.Instance },
            { RetreatGoal.Instance.Name, RetreatGoal.Instance },
            { IgnoreGoal.Instance.Name, IgnoreGoal.Instance }
        };
    }
}
