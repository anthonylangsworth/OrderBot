﻿using System.Net;
using System.Text.Json;

namespace OrderBot.ToDo;

/// <summary>
/// Validate minor factions and star systems for <see cref="ToDoListApi"/>
/// using the APIs from "https://elitebgs.app".
/// </summary>
public class EliteBgsValidator : INameValidator
{
    /// <inheritdoc/>
    public async virtual Task<bool> IsKnownMinorFaction(string minorFactionName)
    {
        return await IsKnown($"https://elitebgs.app/api/ebgs/v5/factions?name={WebUtility.UrlEncode(minorFactionName)}");
    }

    /// <inheritdoc/>
    public async virtual Task<bool> IsKnownStarSystem(string starSystemName)
    {
        return await IsKnown($"https://elitebgs.app/api/ebgs/v5/systems?name={WebUtility.UrlEncode(starSystemName)}");
    }

    private static async Task<bool> IsKnown(string url)
    {
        using HttpClient client = new();
        using Stream stream = await client.GetStreamAsync(url);
        using StreamReader reader = new(stream);
        JsonDocument jsonDocument = await JsonDocument.ParseAsync(stream);
        return jsonDocument.RootElement.GetProperty("docs").GetArrayLength() > 0;
    }
}
