using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderBot.Core;
using OrderBot.Discord;
using OrderBot.EntityFramework;
using OrderBot.MessageProcessors;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Transactions;

namespace OrderBot.CarrierMovement;

/// <summary>
/// Update non-ignored carrier locations and notify Discord Guilds. 
/// Called by <see cref="EddnMessageHostedService"/>.
/// </summary>
public class CarrierMovementMessageProcessor : EddnMessageProcessor
{
    public CarrierMovementMessageProcessor(OrderBotDbContext dbContext,
        ILogger<CarrierMovementMessageProcessor> logger,
        TextChannelWriterFactory textChannelWriterFactory,
        StarSystemToDiscordGuildCache starSystemToDiscordGuildCache,
        IgnoredCarriersCache ignoredCarriersCache,
        CarrierMovementChannelCache carrierMovementChannelCache
    )
    {
        DbContext = dbContext;
        Logger = logger;
        TextChannelWriterFactory = textChannelWriterFactory;
        StarSystemToDiscordGuildCache = starSystemToDiscordGuildCache;
        IgnoredCarriersCache = ignoredCarriersCache;
        CarrierMovementChannelCache = carrierMovementChannelCache;
    }

    internal OrderBotDbContext DbContext { get; }
    internal ILogger<CarrierMovementMessageProcessor> Logger { get; }
    internal TextChannelWriterFactory TextChannelWriterFactory { get; }
    internal StarSystemToDiscordGuildCache StarSystemToDiscordGuildCache { get; }
    internal IgnoredCarriersCache IgnoredCarriersCache { get; }
    internal CarrierMovementChannelCache CarrierMovementChannelCache { get; }

    /// <inheritdoc/>
    public override async Task ProcessAsync(JsonDocument message)
    {
        DateTime timestamp = GetMessageTimestamp(message);

        JsonElement messageElement = message.RootElement.GetProperty("message");
        if (messageElement.TryGetProperty("event", out JsonElement eventProperty)
            && eventProperty.GetString() == "FSSSignalDiscovered"
            && messageElement.TryGetProperty("StarSystem", out JsonElement starSystemProperty)
            && messageElement.TryGetProperty("signals", out JsonElement signalsElement))
        {
            Signal[]? signals = signalsElement.Deserialize<Signal[]>();
            if (signals != null)
            {
                string? starSystemName = starSystemProperty.GetString();
                if (starSystemName != null
                    && StarSystemToDiscordGuildCache.IsMonitoredStarSystem(DbContext, starSystemName))
                {
                    StarSystem? starSystem = DbContext.StarSystems.FirstOrDefault(ss => ss.Name == starSystemName);
                    if (starSystem != null)
                    {
                        IReadOnlyList<Carrier> observedCarriers;
                        using (TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            observedCarriers = UpdateNewCarrierLocations(starSystem, timestamp, signals);

                            // Not all messages are complete. Therefore, we cannot say a carrier has jumped out
                            // if we do not receive a signal for it.
                            // RemoveAbsentCarrierLocations(dbContext, starSystem, discordGuilds, observedCarriers);

                            transactionScope.Complete();
                        }

                        await NotifyCarrierJumps(starSystem,
                            observedCarriers.Where(c => c.FirstSeen == timestamp),
                            StarSystemToDiscordGuildCache.GetGuildsForStarSystem(DbContext, starSystem.Name));
                    }
                }
            }
        }
    }

    /// <summary>
    /// Extract <see cref="Carrier"/s from <paramref name="signals"/> then add or update 
    /// carriers in the database.
    /// </summary>
    /// <param name="starSystem">
    /// The star system.
    /// </param>
    /// <param name="timestamp">
    /// The UTC date and time from the message.
    /// </param>
    /// <param name="signals">
    /// The signals from the message.
    /// </param>
    /// <returns>
    /// All <see cref="Carrier"/>s seen in that system.
    /// </returns>
    private IReadOnlyList<Carrier> UpdateNewCarrierLocations(
        StarSystem starSystem, DateTime timestamp, Signal[] signals)
    {
        List<Carrier> observedCarriers = new();
        foreach (Signal signal in signals.Where(s => s.IsStation && Carrier.IsCarrier(s.Name)))
        {
            string serialNumber = Carrier.GetSerialNumber(signal.Name);
            Carrier? carrier = DbContext.Carriers.Include(c => c.StarSystem)
                                                 .FirstOrDefault(c => c.SerialNumber == serialNumber);
            if (carrier == null)
            {
                carrier = new Carrier() { Name = signal.Name };
                DbContext.Carriers.Add(carrier);
            }
            observedCarriers.Add(carrier);
            if (carrier.StarSystem != starSystem)
            {
                carrier.StarSystem = starSystem;
                carrier.FirstSeen = timestamp;
            }
        }
        DbContext.SaveChanges();
        if (observedCarriers.Any())
        {
            Logger.LogInformation("Carrier(s) {Carriers} in {StarSystem} updated",
                string.Join(", ", observedCarriers.Select(c => c.Name).OrderBy(n => n)), starSystem.Name);
        }
        return observedCarriers.ToArray();
    }

    /// <summary>
    /// Notify discord guilds about carrier movement.
    /// </summary>
    /// <param name="starSystem">
    /// The <see cref="StarSystem"/> the carriers have jumped in.
    /// </param>
    /// <param name="newCarriers">
    /// New carriers that just jumped in.
    /// </param>
    /// <param name="discordGuildToCarrierMovementChannel">
    /// Map Discord guilds to the configured carrier movement channel. Used to 
    /// determine the channel to write to.
    /// </param>
    /// <param name="discordGuildToIgnoredCarrierSerialNumbers">
    /// Map Discord guilds to the serial numbers of ignored carriers. Used to 
    /// avoid notifying about ignored carriers.
    /// </param>
    private async Task NotifyCarrierJumps(StarSystem starSystem,
        IEnumerable<Carrier> newCarriers,
        IReadOnlySet<ulong> discordGuilds)
    {
        foreach (ulong discordGuildId in discordGuilds)
        {
            ulong? carrierMovementChannel = CarrierMovementChannelCache.GetCarrierMovementChannel(DbContext, discordGuildId);
            if (carrierMovementChannel != null)
            {
                try
                {
                    IEnumerable<Carrier> carriersToNotify = newCarriers.Where(c => !IgnoredCarriersCache.IsIgnored(DbContext, discordGuildId, c.SerialNumber));
                    if (carriersToNotify.Any())
                    {
                        using TextWriter textChannelWriter =
                            await TextChannelWriterFactory.GetWriterAsync(carrierMovementChannel);
                        textChannelWriter.Write(GetCarrierMovementMessage(starSystem, carriersToNotify));
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(
                        ex,
                        "Sending carrier jump notification(s) to channel '{ChannelId}' for discord guild '{GuildId}' failed",
                        carrierMovementChannel, discordGuildId
                    );
                }
            }
        }
    }

    /// <summary>
    /// Construct the detail line for each carrier written to <see cref="DiscordGuild.CarrierMovementChannel"/> on a carrier jump.
    /// </summary>
    /// <param name="carrier">
    /// The carrier to write details for.
    /// </param>
    /// <returns>
    /// The message.
    /// </returns>
    internal static string GetCarrierMovementMessage(StarSystem starSystem, IEnumerable<Carrier> carriers)
    {
        StringBuilder stringBuilder = new();
        if (carriers.Any())
        {
            stringBuilder.AppendLine($"New fleet carriers in {starSystem.Name} (<https://inara.cz/elite/search/?search={WebUtility.UrlEncode(starSystem.Name)}>):");
            foreach (Carrier carrier in carriers.OrderBy(c => c.Name))
            {
                stringBuilder.AppendLine($"- {carrier.Name} (<https://inara.cz/elite/search/?search={WebUtility.UrlEncode(carrier.SerialNumber)}>)");
            }
        }
        return stringBuilder.ToString().Trim();
    }

    // Not all messages are complete. Therefore, we cannot say a carrier has jumped out
    // if we do not receive a signal for it.
    //private void RemoveAbsentCarrierLocations(OrderBotDbContext dbContext, StarSystem starSystem, IReadOnlyList<DiscordGuild> discordGuilds, Carrier[] observedCarriers)
    //{
    //    foreach (Carrier carrier in dbContext.Carriers.Where(c => c.StarSystem == starSystem && !observedCarriers.Contains(c)))
    //    {
    //        carrier.StarSystem = null;
    //        carrier.FirstSeen = null;

    //        foreach (DiscordGuild discordGuild in
    //            discordGuilds.Where(dg => !dg.IgnoredCarriers.Any(c => c.SerialNumber == carrier.SerialNumber)))
    //        {
    //            if (DiscordClient.GetChannel(discordGuild.CarrierMovementChannel ?? 0) is ISocketMessageChannel channel)
    //            {
    //                try
    //                {
    //                    channel.SendMessageAsync(text: $"Fleet carrier '{carrier.Name}' has left '{starSystem.Name}'. Inara: https://inara.cz/elite/search/?search={carrier.SerialNumber}").GetAwaiter().GetResult();
    //                }
    //                catch (Exception ex)
    //                {
    //                    Logger.LogError(ex, "Updating channel '{channelId}' for discord Guid '{discordGuildId}' failed", channel.Id, discordGuild.Id);
    //                }
    //            }
    //        }
    //    }
    //    dbContext.SaveChanges();
    //}
}
