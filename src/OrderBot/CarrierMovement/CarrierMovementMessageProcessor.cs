using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderBot.Core;
using OrderBot.Discord;
using OrderBot.EntityFramework;
using OrderBot.MessageProcessors;
using System.Net;
using System.Text.Json;
using System.Transactions;

namespace OrderBot.CarrierMovement;

/// <summary>
/// Update non-ignored carrier locations and notify Discord Guilds. Called by <see cref="EddnMessageHostedService"/>.
/// </summary>
public class CarrierMovementMessageProcessor : EddnMessageProcessor, IDisposable
{
    public CarrierMovementMessageProcessor(OrderBotDbContext dbContext,
        ILogger<CarrierMovementMessageProcessor> logger, IDiscordClient discordClient,
        IMemoryCache memoryCache, IOptions<DiscordClientConfig> config)
    {
        DbContext = dbContext;
        Logger = logger;
        DiscordClient = discordClient;
        MemoryCache = memoryCache;

        if (DiscordClient.ConnectionState != ConnectionState.Connected
            && DiscordClient is DiscordSocketClient discordSocketClient)
        {
            _stopDiscordClient = true;
            discordSocketClient.LoginAsync(TokenType.Bot, config.Value.ApiKey).GetAwaiter().GetResult();
            discordSocketClient.StartAsync().GetAwaiter().GetResult();
        }
        else
        {
            _stopDiscordClient = false;
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                if (_stopDiscordClient
                    && DiscordClient is DiscordSocketClient discordSocketClient)
                {
                    discordSocketClient.StopAsync().GetAwaiter().GetResult();
                    _stopDiscordClient = false;
                }
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }


    public OrderBotDbContext DbContext { get; }
    public ILogger<CarrierMovementMessageProcessor> Logger { get; }
    public IDiscordClient DiscordClient { get; }
    public IMemoryCache MemoryCache { get; }

    public static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    private bool _stopDiscordClient;
    private bool disposedValue;

    /// <inheritdoc/>
    public override async Task ProcessAsync(JsonDocument message)
    {
        DateTime timestamp = GetMessageTimestamp(message);

        // See https://github.com/EDCD/EDDN/blob/master/schemas/fsssignaldiscovered-v1.0.json for the schema
        // "signals": [{"IsStation": true, "SignalName": "THE PEAKY BLINDERS KNF-83G", "timestamp": "2022-10-13T12:13:09Z"}]

        // Maps the star system name to a list of discord guild IDs and the carrier movement channel IDs
        IDictionary<string, IDictionary<int, ulong?>> starSystemToDiscordGuildToCarrierMovementChannel =
            GetStarSystemToDiscordGuildToCarrierMovementChannel();
        IDictionary<int, List<string>> discordGuildToIgnoredCarrierSerialNumber = GetIgnoredCarriers();

        //IList<DiscordGuildPresenceGoal> discordGuildPresenceGoals = MemoryCache.GetOrCreate(
        //    $"{nameof(CarrierMovementMessageProcessor)}_DiscordGuildPresenceGoals",
        //    ce =>
        //    {
        //        ce.AbsoluteExpiration = DateTime.Now.Add(CacheDuration);
        //        // Logger.LogInformation("Cache entry {Key} refreshed after {CacheDuration}", ce.Key, CacheDuration);
        //        return DbContext.DiscordGuildPresenceGoals.Include(dgpg => dgpg.DiscordGuild)
        //                                                  .Include(dgpg => dgpg.DiscordGuild.IgnoredCarriers)
        //                                                  .Include(dgpg => dgpg.Presence)
        //                                                  .Include(dgpg => dgpg.Presence.StarSystem)
        //                                                  .ToList();
        //    });
        //IList<Presence> presences = MemoryCache.GetOrCreate(
        //    $"{nameof(CarrierMovementMessageProcessor)}_Presences",
        //    ce =>
        //    {
        //        ce.AbsoluteExpiration = DateTime.Now.Add(CacheDuration);
        //        // Logger.LogInformation("Cache entry {Key} refreshed after {CacheDuration}", ce.Key, CacheDuration);
        //        return DbContext.Presences.Include(p => p.MinorFaction)
        //                                  .Include(p => p.MinorFaction.SupportedBy)
        //                                  .Include(p => p.StarSystem)
        //                                  .ToList();
        //    });

        JsonElement messageElement = message.RootElement.GetProperty("message");
        if (messageElement.TryGetProperty("event", out JsonElement eventProperty)
            && eventProperty.GetString() == "FSSSignalDiscovered"
            && messageElement.TryGetProperty("StarSystem", out JsonElement starSystemProperty)
            && messageElement.TryGetProperty("signals", out JsonElement signalsElement))
        {
            Signal[]? signals = signalsElement.Deserialize<Signal[]>();
            if (signals != null)
            {
                string starSystemName = starSystemProperty.GetString() ?? "";
                if (starSystemToDiscordGuildToCarrierMovementChannel.ContainsKey(starSystemName))
                {
                    StarSystem? starSystem = DbContext.StarSystems.FirstOrDefault(ss => ss.Name == starSystemName);
                    if (starSystem != null)
                    {
                        using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);
                        IReadOnlyList<Carrier> observedCarriers = UpdateNewCarrierLocations(starSystem, timestamp, signals);

                        // Not all messages are complete. Therefore, we cannot say a carrier has jumped out
                        // if we do not receive a signal for it.
                        // RemoveAbsentCarrierLocations(dbContext, starSystem, discordGuilds, observedCarriers);

                        transactionScope.Complete();

                        await NotifyCarrierJumps(starSystem, observedCarriers,
                            starSystemToDiscordGuildToCarrierMovementChannel[starSystemName],
                            discordGuildToIgnoredCarrierSerialNumber);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Get a mapping of discord guilds to the serial numbers of ignored carriers.
    /// </summary>
    /// <returns>
    /// The mapping.
    /// </returns>
    private IDictionary<int, List<string>> GetIgnoredCarriers()
    {
        return DbContext.DiscordGuilds.Include(dg => dg.IgnoredCarriers)
                                   .ToDictionary(dg => dg.Id, dg => dg.IgnoredCarriers.Select(ic => ic.SerialNumber).ToList());
    }

    /// <summary>
    /// Get a mapping of star systems to Discord guilds to carrier movement channels.
    /// </summary>
    /// <remarks>
    /// Intentionally return <see cref="DiscordGuild"/>s that do not have the carrier movement channel set.
    /// We still want to save carrier locations if future use requires it.
    /// </remarks>
    /// <returns>
    /// The mapping.
    /// </returns>
    private IDictionary<string, IDictionary<int, ulong?>> GetStarSystemToDiscordGuildToCarrierMovementChannel()
    {
        Dictionary<string, IDictionary<int, ulong?>> result = new();
        IEnumerable<(string Name, int Id, ulong? CarrierMovementChannel)> fromGoals =
            DbContext.DiscordGuildPresenceGoals.Include(dgpg => dgpg.DiscordGuild)
                                               // .Include(dgpg => dgpg.DiscordGuild.IgnoredCarriers)
                                               .Include(dgpg => dgpg.Presence)
                                               .Include(dgpg => dgpg.Presence.StarSystem)
                                               .ToList()
                                               .Select(dgpg => (dgpg.Presence.StarSystem.Name, dgpg.DiscordGuild.Id, dgpg.DiscordGuild.CarrierMovementChannel));
        IEnumerable<(string, int, ulong?)> fromPresences =
            DbContext.Presences.Include(p => p.MinorFaction)
                               .Include(p => p.MinorFaction.SupportedBy)
                               .Include(p => p.StarSystem)
                               .ToList()
                               .SelectMany(p => p.MinorFaction.SupportedBy.Select(dg => (p.StarSystem.Name, dg.Id, dg.CarrierMovementChannel)));
        foreach ((string systemName, int discordGuidId, ulong? carrierMovementChannel) in Enumerable.Concat(fromGoals, fromPresences))
        {
            if (result.TryGetValue(systemName, out IDictionary<int, ulong?>? discordGuildToCarrierMovementChannel))
            {
                if (!discordGuildToCarrierMovementChannel.ContainsKey(discordGuidId))
                {
                    discordGuildToCarrierMovementChannel.Add(discordGuidId, carrierMovementChannel);
                }
            }
            else
            {
                result.Add(systemName, new Dictionary<int, ulong?>() { { discordGuidId, carrierMovementChannel } });
            }
        }
        return result;
    }

    /// <summary>
    /// Update carrier locations in the database.
    /// </summary>
    /// <param name="starSystem">
    /// The star system.
    /// </param>
    /// <param name="timestamp">
    /// The date time from the message.
    /// </param>
    /// <param name="signals">
    /// The signals from the signal source.
    /// </param>
    /// <returns>
    /// The new <see cref="Carrier"/>s.
    /// </returns>
    internal IReadOnlyList<Carrier> UpdateNewCarrierLocations(
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
                string.Join(", ", observedCarriers.Select(c => c.Name)), starSystem.Name);
        }
        return observedCarriers.ToArray();
    }

    /// <summary>
    /// Notify discord guilds about carrier movement.
    /// </summary>
    /// <param name="starSystem">
    /// The <see cref="StarSystem"/> the carriers have jumped in.
    /// </param>
    /// <param name="observedCarriers">
    /// The carriers that jumped in.
    /// </param>
    /// <param name="discordGuildToCarrierMovementChannel">
    /// Map Discord guilds to the configured carrier movement channel. Used to 
    /// determine the channel to write to.
    /// </param>
    /// <param name="discordGuildToIgnoredCarrierSerialNumbers">
    /// Map Discord guilds to the serial numbers of ignored carriers. Used to 
    /// avoid notifying about ignored carriers.
    /// </param>
    internal async Task NotifyCarrierJumps(StarSystem starSystem, IReadOnlyList<Carrier> observedCarriers,
        IDictionary<int, ulong?> discordGuildToCarrierMovementChannel,
        IDictionary<int, List<string>> discordGuildToIgnoredCarrierSerialNumbers)
    {
        foreach (int discordGuildId in discordGuildToCarrierMovementChannel.Keys)
        {
            discordGuildToIgnoredCarrierSerialNumbers.TryGetValue(discordGuildId, out List<string>? ignoredCarriers);
            ignoredCarriers ??= new List<string>();
            if (discordGuildToCarrierMovementChannel.TryGetValue(discordGuildId, out ulong? carrierMovementChannel)
               && await DiscordClient.GetChannelAsync(carrierMovementChannel ?? 0) is ITextChannel channel)
            {
                try
                {
                    using TextChannelWriter textChannelWriter = new(channel);
                    foreach (Carrier carrier in observedCarriers.Where(c => !ignoredCarriers.Contains(c.SerialNumber))
                                                                .OrderBy(c => c.Name))
                    {
                        textChannelWriter.WriteLine(GetCarrierMovementMessage(carrier, starSystem));
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(
                        ex, "Sending carrier notification to channel '{ChannelId}' for discord Guid '{GuildId}' failed",
                        channel.Id, discordGuildId
                    );
                }
            }
        }
    }

    /// <summary>
    /// Construct the message written to <see cref="DiscordGuild.CarrierMovementChannel"/> on a carrier jump.
    /// </summary>
    /// <param name="carrier">
    /// The carrier to write details for.
    /// </param>
    /// <param name="starSystem">
    /// The star system to write details for.
    /// </param>
    /// <returns>
    /// The message.
    /// </returns>
    internal static string GetCarrierMovementMessage(Carrier carrier, StarSystem starSystem)
    {
        return $"New fleet carrier '{carrier.Name}' (<https://inara.cz/elite/search/?search={WebUtility.UrlEncode(carrier.SerialNumber)}>) seen in '{starSystem.Name}' (<https://inara.cz/elite/search/?search={WebUtility.UrlEncode(starSystem.Name)}>).";
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
