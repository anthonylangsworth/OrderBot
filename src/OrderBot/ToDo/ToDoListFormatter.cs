namespace OrderBot.ToDo
{
    public class ToDoListFormatter
    {
        internal string GetOutput(string minorFactionName, string proList, string proSecurityList, string antiList, string otherList, string warList, string electionList) =>
$@"---------------------------------------------------------------------------------------------------------------------------------
***Pro-{minorFactionName}** support required* - Work for EDA in these systems.
E.g. Missions/PAX, cartographic data, bounties, and profitable trade to *{minorFactionName}* controlled stations.
{proList}

Redeem bounty vouchers to increase security in systems *{minorFactionName}* controls.
{proSecurityList}

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

        internal static string GetInfluenceList(IEnumerable<InfluenceInitiatedSuggestion> suggestions, bool ascending)
        {
            string result;
            if (suggestions.Any())
            {
                IEnumerable<InfluenceInitiatedSuggestion> sortedActions =
                    ascending ? suggestions.OrderBy(action => action.Influence) : suggestions.OrderByDescending(action => action.Influence);
                result = string.Join(Environment.NewLine,
                    sortedActions.Select(action => $"- {FormatSystemName(action.StarSystem.Name)} - {Math.Round(action.Influence * 100, 1)}%"));
            }
            else
            {
                result = "(None)";
            }
            return result;
        }

        internal static string GetSecurityList(IEnumerable<SecurityInitiatedSuggestion> suggestions)
        {
            string result;
            if (suggestions.Any())
            {
                result = string.Join(Environment.NewLine,
                    suggestions.OrderBy(sis => sis.StarSystem.Name)
                               .Select(action => $"- {FormatSystemName(action.StarSystem.Name)} - {SecurityLevel.Name[action.SecurityLevel]}"));
            }
            else
            {
                result = "(None)";
            }
            return result;
        }

        internal static string FormatSystemName(string systemName)
        {
            // Temporarily removed because this bumped the order message size over the 2K character limit.
            // The < > prevent auto-embed creation for the links.
            // return $"[{systemName}](<https://inara.cz/elite/search/?search={WebUtility.UrlEncode(systemName)}>)";
            return $"{systemName}";
        }

        public string Format(ToDoList toDoList)
        {
            return GetOutput(toDoList.MinorFaction, GetInfluenceList(toDoList.Pro, true), GetSecurityList(toDoList.ProSecurity), GetInfluenceList(toDoList.Anti, false), "(None)", "(None)", "(None)");
        }
    }
}
