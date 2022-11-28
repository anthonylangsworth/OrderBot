using Discord;
using OrderBot.Core;
using OrderBot.EntityFramework;

namespace OrderBot.CarrierMovement;

/// <summary>
/// Suggest carriers that are NOT on the ignored carrier list for the Discord guild.
/// </summary>
public class NotIgnoredCarriersAutocompleteHandler : CarriersAutocompleteHandler
{
    /// <summary>
    /// Create a new <see cref="NotIgnoredCarriersAutocompleteHandler"/>.
    /// </summary>
    /// <param name="dbContextFactory">
    /// The database to check.
    /// </param>
    public NotIgnoredCarriersAutocompleteHandler(OrderBotDbContext dbContext)
        : base(dbContext)
    {
        // Do nothing
    }

    /// <inheritdoc/>
    protected internal override IEnumerable<string> GetCarriers(OrderBotDbContext dbContext,
        DiscordGuild discordGuild, string startsWith)
    {
        // max - 25 suggestions at a time (API limit)
        return dbContext.Carriers.Where(c => !discordGuild.IgnoredCarriers.Contains(c) && c.Name.StartsWith(startsWith))
                                 .Select(c => c.Name)
                                 .OrderBy(cn => cn)
                                 .Take(SlashCommandBuilder.MaxOptionsCount)
                                 .ToList();
    }
}
