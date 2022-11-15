using Discord;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using OrderBot.Core;
using OrderBot.Discord;
using OrderBot.EntityFramework;
using OrderBot.MessageProcessors;
using System.Net;
using System.Text.Json;
using System.Transactions;

namespace OrderBot.CarrierMovement;

public class CarrierMovementMessageProcessor : EddnMessageProcessor
{
    public CarrierMovementMessageProcessor(IDbContextFactory<OrderBotDbContext> contextFactory,
        ILogger<CarrierMovementMessageProcessor> logger, IDiscordClient discordClient,
        IMemoryCache memoryCache)
    {
        ContextFactory = contextFactory;
        Logger = logger;
        DiscordClient = discordClient;
        MemoryCache = memoryCache;
    }

    public IDbContextFactory<OrderBotDbContext> ContextFactory { get; }
    public ILogger<CarrierMovementMessageProcessor> Logger { get; }
    public IDiscordClient DiscordClient { get; }
    public IMemoryCache MemoryCache { get; }

    public static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    /// <inheritdoc/>
    public override async Task ProcessAsync(JsonDocument message)
    {
        DateTime timestamp = message.RootElement
                .GetProperty("header")
                .GetProperty("gatewayTimestamp")
                .GetDateTime()
                .ToUniversalTime();

        // See https://github.com/EDCD/EDDN/blob/master/schemas/fsssignaldiscovered-v1.0.json for the schema
        // "signals": [{"IsStation": true, "SignalName": "THE PEAKY BLINDERS KNF-83G", "timestamp": "2022-10-13T12:13:09Z"}]

        using OrderBotDbContext dbContext = ContextFactory.CreateDbContext();
        IList<DiscordGuildPresenceGoal> discordGuildPresenceGoals = MemoryCache.GetOrCreate(
            $"{nameof(CarrierMovementMessageProcessor)}_DiscordGuildPresenceGoals",
            ce =>
            {
                ce.AbsoluteExpiration = DateTime.Now.Add(CacheDuration);
                return dbContext.DiscordGuildPresenceGoals.Include(dgpg => dgpg.DiscordGuild)
                                                          .Include(dgpg => dgpg.DiscordGuild.IgnoredCarriers)
                                                          .Include(dgpg => dgpg.Presence)
                                                          .Include(dgpg => dgpg.Presence.StarSystem)
                                                          .ToList();
            });
        IList<Presence> presences = MemoryCache.GetOrCreate(
            $"{nameof(CarrierMovementMessageProcessor)}_Presences",
            ce =>
            {
                ce.AbsoluteExpiration = DateTime.Now.Add(CacheDuration);
                return dbContext.Presences.Include(p => p.MinorFaction)
                                          .Include(p => p.MinorFaction.SupportedBy)
                                          .Include(p => p.StarSystem)
                                          .ToList();
            });
        IList<StarSystem> starSystems =
            Enumerable.Concat(
                discordGuildPresenceGoals.Select(dgpg => dgpg.Presence.StarSystem),
                presences.Select(p => p.StarSystem)
             ).Distinct()
              .ToList();
        IList<DiscordGuild> discordGuilds =
            Enumerable.Concat(
                discordGuildPresenceGoals.Select(dgpg => dgpg.DiscordGuild),
                presences.SelectMany(p => p.MinorFaction.SupportedBy)
            ).Distinct()
             .Where(dg => dg.CarrierMovementChannel != null)
             .ToList();

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
                StarSystem? starSystem = starSystems.FirstOrDefault(ss => ss.Name == starSystemName);
                if (starSystem != null)
                {
                    using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);
                    IReadOnlyList<Carrier> observedCarriers = UpdateNewCarrierLocations(dbContext, starSystem, timestamp, signals);
                    await NotifyCarrierJumps(starSystem, observedCarriers, discordGuildPresenceGoals, presences);
                    // Not all messages are complete. Therefore, we cannot say a carrier has jumped out
                    // if we do not receive a signal for it.
                    // RemoveAbsentCarrierLocations(dbContext, starSystem, discordGuilds, observedCarriers);
                    transactionScope.Complete();
                }
            }
        }
    }

    /// <summary>
    /// Update carrier locations in the database.
    /// </summary>
    /// <param name="dbContext">
    /// The database.
    /// </param>
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
    internal IReadOnlyList<Carrier> UpdateNewCarrierLocations(OrderBotDbContext dbContext,
        StarSystem starSystem, DateTime timestamp, Signal[] signals)
    {
        List<Carrier> observedCarriers = new();
        foreach (Signal signal in signals.Where(s => s.IsStation && Carrier.IsCarrier(s.Name)))
        {
            string serialNumber = Carrier.GetSerialNumber(signal.Name);
            Carrier? carrier = dbContext.Carriers.Include(c => c.StarSystem)
                                                 .FirstOrDefault(c => c.SerialNumber == serialNumber);
            if (carrier == null)
            {
                carrier = new Carrier() { Name = signal.Name };
                dbContext.Carriers.Add(carrier);
            }
            observedCarriers.Add(carrier);
            if (carrier.StarSystem != starSystem)
            {
                carrier.StarSystem = starSystem;
                carrier.FirstSeen = timestamp;
            }
        }
        dbContext.SaveChanges();
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
    /// <param name="discordGuildPresenceGoals">
    /// Notify the server has one or more goals for this system.
    /// </param>
    /// <param name="presences">
    /// Notify the server supports a minor faction present in this system.
    /// </param>
    internal async Task NotifyCarrierJumps(StarSystem starSystem, IReadOnlyList<Carrier> observedCarriers,
        IList<DiscordGuildPresenceGoal> discordGuildPresenceGoals, IList<Presence> presences)
    {
        foreach (DiscordGuild discordGuild in discordGuildPresenceGoals.Select(dgpg => dgpg.DiscordGuild).Distinct())
        {
            if (await DiscordClient.GetChannelAsync(discordGuild.CarrierMovementChannel ?? 0) is ITextChannel channel
                && (discordGuildPresenceGoals.Any(dgpg => dgpg.DiscordGuild == discordGuild && dgpg.Presence.StarSystem == starSystem)
                    || presences.Any(p => p.MinorFaction.SupportedBy.Contains(discordGuild) && p.StarSystem == starSystem)))
            {
                try
                {
                    using TextChannelWriter textChannelWriter = new(channel);
                    foreach (Carrier carrier in observedCarriers.Except(discordGuild.IgnoredCarriers).OrderBy(c => c.Name))
                    {
                        textChannelWriter.WriteLine(GetCarrierMovementMessage(carrier, starSystem));
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(
                        ex, "Sending carrier notification to channel '{ChannelId}' for discord Guid '{GuildId}' failed",
                        channel.Id, discordGuild.Id
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
