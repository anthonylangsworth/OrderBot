using OrderBot.Core;

namespace OrderBot.Reports
{
    internal class ControlGoal : Goal
    {
        /// <summary>
        /// Singleton.
        /// </summary>
        public static ControlGoal Instance = new();

        /// <summary>
        /// Create a new <see cref="ControlGoal"/>.
        /// </summary>
        private ControlGoal()
            : base("Control", "Be the highest influence minor faction. Keep influence between 50% and 60%.")
        {
            // Do nothing
        }

        /// <summary>
        /// Work for this minor faction if the influence drops below this level.
        /// </summary>
        public double LowerThreshold = 0.55;

        /// <summary>
        /// Work against this minor faction if the influence raises above this level.
        /// </summary>
        public double UpperThreshold = 0.65;

        /// <inheritdoc/>
        public override void AddActions(StarSystemMinorFaction starSystemMinorFaction, ToDoList toDoList)
        {
            if (starSystemMinorFaction.Influence < LowerThreshold)
            {
                toDoList.Pro.Add(new(starSystemMinorFaction.StarSystem, starSystemMinorFaction.Influence));
            }
            else if (starSystemMinorFaction.Influence > UpperThreshold)
            {
                toDoList.Anti.Add(new(starSystemMinorFaction.StarSystem, starSystemMinorFaction.Influence));
            }

            // TODO: Handle conflicts
        }
    }
}
