using System.Net;
using System.Text.Json;

namespace OrderBot.ToDo;

/// <summary>
/// Validate minor factions and star systems for <see cref="ToDoListApi"/>.
/// </summary>
public class Validator
{
    /// <summary>
    /// Is <paramref name="minorFactionName"/> a valid minor faction?
    /// </summary>
    /// <param name="minorFactionName">
    /// The name to test.
    /// </param>
    /// <returns>
    /// <c>true</c> if it is known, <c>false</c> otherwise.
    /// </returns>
    public async virtual Task<bool> IsKnownMinorFactionAsync(string minorFactionName)
    {
        return await IsKnown($"https://elitebgs.app/api/ebgs/v5/factions?name={WebUtility.UrlEncode(minorFactionName)}");
    }

    /// <summary>
    /// Is <paramref name="starSystemName"/> a valid star system?
    /// </summary>
    /// <param name="starSystemName">
    /// The name to test.
    /// </param>
    /// <returns>
    /// <c>true</c> if it is known, <c>false</c> otherwise.
    /// </returns>
    public async virtual Task<bool> IsKnownStarSystemAsync(string starSystemName)
    {
        return await IsKnown($"https://elitebgs.app/api/ebgs/v5/systems?name={WebUtility.UrlEncode(starSystemName)}");
    }

    private static async Task<bool> IsKnown(string url)
    {
        using HttpClient client = new();
        using Stream stream = await client.GetStreamAsync(url);
        using StreamReader reader = new(stream);
        JsonDocument jsonDocument = JsonDocument.Parse(stream);
        return jsonDocument.RootElement.GetProperty("docs").GetArrayLength() > 0;
    }
}
