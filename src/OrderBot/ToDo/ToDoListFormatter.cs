namespace OrderBot.ToDo
{
    public class ToDoListFormatter
    {
        internal string GetOutput(string minorFactionName, string proList, string antiList, string otherList, string warList, string electionList) =>
$@"---------------------------------------------------------------------------------------------------------------------------------
***Pro-{minorFactionName}** support required* - Work for EDA in these systems.
Missions/PAX, Cartographic Data, Bounties, and Profitable Trade to *{minorFactionName}* controlled stations:
{proList}

***Anti-{minorFactionName}** support required* - Work ONLY for the other factions in the listed systems to bring *{minorFactionName}*'s INF back to manageable levels and to avoid an unwanted expansion.
{antiList}

***Urgent Pro-Non-Native/Coalition Faction** support required* - Work for ONLY the listed factions in the listed systems to avoid a retreat or to disrupt system interference.
{otherList}

---------------------------------------------------------------------------------------------------------------------------------
**War Systems**
{warList}

**Election Systems**
{electionList}
";

        internal static string GetInfluenceList(IEnumerable<InfluenceInitiatedSuggestion> actions, bool ascending)
        {
            string result;
            if (actions.Any())
            {
                IEnumerable<InfluenceInitiatedSuggestion> sortedActions =
                    ascending ? actions.OrderBy(action => action.Influence) : actions.OrderByDescending(action => action.Influence);
                result = string.Join(Environment.NewLine,
                    sortedActions.Select(action => $"- {action.StarSystem.Name} - {Math.Round(action.Influence * 100, 1)}%"));
            }
            else
            {
                result = "(None)";
            }
            return result;
        }

        public string Format(ToDoList toDoList)
        {
            return GetOutput(toDoList.MinorFaction, GetInfluenceList(toDoList.Pro, true), GetInfluenceList(toDoList.Anti, false), "(None)", "(None)", "(None)");
        }
    }
}
