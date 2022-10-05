namespace OrderBot.Reports
{
    internal class Goals
    {
        // public static Goal Ignore => new Goal("Ignore", "Generate no orders for this system. Useful systems that you do not care about.");

        // public static Goal Maintain => new Goal("Control", "Neither retreat nor control. Aim to keep influence up to 10% less than the controlling minor faction.");

        // public static Goal Retreat => new Goal("Retreat", "Retreat from the system by reducing influence below 5% and keeping it there.");

        public static Goal Default => ControlGoal.Instance;

        public static IDictionary<string, Goal> Map = new Dictionary<string, Goal>
        {
            { ControlGoal.Instance.Name, ControlGoal.Instance }
        };
    }
}
