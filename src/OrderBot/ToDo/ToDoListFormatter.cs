using OrderBot.Core;

namespace OrderBot.ToDo;

/// <summary>
/// Convert a <see cref="ToDoList"/> to a human-readable form.
/// </summary>
public class ToDoListFormatter
{
    /// <summary>
    /// Shown when there are no rows in each category.
    /// </summary>
    internal readonly static string None = "(None)";

    /// <summary>
    /// The max rows of each category returned. This prevents
    /// the suggestions from getting too long.
    /// </summary>
    internal readonly static int maxRows = 8;

    /// <summary>
    /// Format the <paramref name="toDoList"/> to a human-readable form.
    /// </summary>
    /// <param name="toDoList">
    /// The <see cref="ToDoList"/> to convert.
    /// </param>
    /// <returns>
    /// A human-readable list of suggestipons.
    /// </returns>
    public string Format(ToDoList toDoList)
    {
        Func<ToDoList, string>[] output = new[]
        {
            Line,
            ProInfluence,
            BlankLine,
            // Security,
            // BlankLine,
            AntiInfluence,
            BlankLine,
            OtherInfluence,
            BlankLine,
            Line,
            Wars,
            BlankLine,
            Elections,
            BlankLine
        };
        return string.Join(
            Environment.NewLine,
            output.Select(o => o(toDoList)));
    }

    internal static string BlankLine(ToDoList toDoList)
    {
        return "";
    }

    internal static string Line(ToDoList toDoList)
    {
        return "---------------------------------------------------------------------------------------------------------------------------------";
    }

    internal static string ProInfluence(ToDoList toDoList)
    {
        return
$@"***Pro-{toDoList.MinorFaction}** support required* - Work for *{toDoList.MinorFaction}* in these systems.
E.g. Missions/PAX, cartographic data, bounties, and profitable trade to *{toDoList.MinorFaction}* controlled stations.
{GetInfluenceList(toDoList, i => i.Pro, ascending: true)}";
    }

    internal static string AntiInfluence(ToDoList toDoList)
    {
        return
$@"***Anti-{toDoList.MinorFaction}** support required* - Work ONLY for the other factions in the listed systems to bring *{toDoList.MinorFaction}*'s INF back to manageable levels and to avoid an unwanted expansion.
{GetInfluenceList(toDoList, i => !i.Pro, ascending: false)}";
    }

    internal static string OtherInfluence(ToDoList toDoList)
    {
        return
$@"***Urgent Pro-Non-Native/Coalition Faction** support required* - Work for ONLY the listed factions in the listed systems to avoid a retreat or to disrupt system interference.
{GetInfluenceList(toDoList, i => false, ascending: true)}";
    }

    internal static string Security(ToDoList toDoList)
    {
        IEnumerable<SecuritySuggestion> suggestions =
            toDoList.Suggestions.Where(s => s is SecuritySuggestion)
                                .Cast<SecuritySuggestion>()
                                .Take(maxRows);
        string proSecurityList;
        if (suggestions.Any())
        {
            proSecurityList = string.Join(Environment.NewLine,
                suggestions.OrderBy(sis => sis.StarSystem.Name)
                           .Select(action => $"- {FormatSystemName(action.StarSystem.Name)} - {SecurityLevel.Name[action.SecurityLevel]}"));
        }
        else
        {
            proSecurityList = None;
        }
        return
$@"Redeem bounty vouchers to increase security in systems *{toDoList.MinorFaction}* controls.
{proSecurityList}";
    }

    internal static string Wars(ToDoList toDoList)
    {
        return
$@"**War Systems**
{GetWarList(toDoList, cs => Conflict.IsWar(cs.WarType))}";
    }

    internal static string Elections(ToDoList toDoList)
    {
        return
$@"**Election Systems**
{GetWarList(toDoList, cs => Conflict.IsElection(cs.WarType))}";
    }

    internal static string GetInfluenceList(ToDoList toDoList, Predicate<InfluenceSuggestion> include, bool ascending)
    {
        IEnumerable<InfluenceSuggestion> suggestions =
            toDoList.Suggestions.Where(s => s is InfluenceSuggestion infSuggestion && include(infSuggestion))
                                .Cast<InfluenceSuggestion>()
                                .Take(maxRows);
        string result;
        if (suggestions.Any())
        {
            IEnumerable<InfluenceSuggestion> sortedActions =
                ascending ? suggestions.OrderBy(action => action.Influence) : suggestions.OrderByDescending(action => action.Influence);
            result = string.Join(Environment.NewLine,
                sortedActions.Select(action => $"- {FormatSystemName(action.StarSystem.Name)} - {Math.Round(action.Influence * 100, 1)}%{ShowDescription(action)}"));
        }
        else
        {
            result = None;
        }

        return result;
    }

    internal static string GetWarList(ToDoList toDoList, Predicate<ConflictSuggestion> include)
    {
        IEnumerable<ConflictSuggestion> suggestions =
            toDoList.Suggestions.Where(s => s is ConflictSuggestion cs && include(cs))
                                .Cast<ConflictSuggestion>()
                                .Take(maxRows);
        string result;
        if (suggestions.Any())
        {
            result = string.Join(Environment.NewLine,
                suggestions.OrderBy(cs => cs.FightFor.Name)
                           .OrderBy(cs => cs.StarSystem.Name)
                           .Select(cs => $"- {FormatSystemName(cs.StarSystem.Name)} - Fight for *{cs.FightFor.Name}* against *{cs.FightAgainst.Name}* - {cs.FightForWonDays} vs {cs.FightAgainstWonDays} (*{cs.State}*){ShowDescription(cs)}"));
        }
        else
        {
            result = None;
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

    internal static string ShowDescription(Suggestion suggestion)
    {
        return string.IsNullOrWhiteSpace(suggestion.Description) ? string.Empty : $" ({suggestion.Description})";
    }
}
