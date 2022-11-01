using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderBot.Core;
using OrderBot.EntityFramework;
using OrderBot.MessageProcessors;
using System.Text.Json;
using System.Transactions;

namespace OrderBot.ToDo
{
    internal class ToDoListMessageProcessor : EddnMessageProcessor
    {
        public ToDoListMessageProcessor(ILogger<ToDoListMessageProcessor> logger,
            IDbContextFactory<OrderBotDbContext> dbContextFactory, MinorFactionNameFilter filter)
        {
            Logger = logger;
            DbContextFactory = dbContextFactory;
            Filter = filter;
        }

        public ILogger<ToDoListMessageProcessor> Logger { get; }
        public IDbContextFactory<OrderBotDbContext> DbContextFactory { get; }
        public MinorFactionNameFilter Filter { get; }

        public override void Process(JsonDocument message)
        {
            using OrderBotDbContext dbContext = DbContextFactory.CreateDbContext();
            using TransactionScope transactionScope = new();

            EddnStarSystemData? bgsSystemData = GetBgsData(message, Filter);
            if (bgsSystemData != null)
            {
                //IExecutionStrategy executionStrategy = dbContext.Database.CreateExecutionStrategy();
                //executionStrategy.Execute(() => InnerSink(timestamp, starSystemName, minorFactionDetails, dbContext));
                Update(dbContext, bgsSystemData);

                Logger.LogInformation("System {system} updated", bgsSystemData.StarSystemName);
            }

            transactionScope.Complete();
        }

        /// <summary>
        /// Extract the timestamp and info for relevant minor factions.
        /// </summary>
        /// <param name="message">
        /// The message received from EDDN.
        /// </param>
        /// <param name="minorFactionNameFilters">
        /// Filter out systems that do not match this filter.
        /// </param>
        /// <returns>
        /// A <see cref="EddnStarSystemData"/> representing the details to store. If null, 
        /// there are no relevant details.
        /// </returns>
        /// <exception cref="JsonException">
        /// The message is not valid JSON.
        /// </exception>
        /// <exception cref="KeyNotFoundException">
        /// The message is valid JSON but is not the expected format.
        /// </exception>
        /// <exception cref="FormatException">
        /// One or more fields are not of the expected format.
        /// </exception>
        internal static EddnStarSystemData? GetBgsData(JsonDocument message,
            MinorFactionNameFilter minorFactionNameFilters)
        {
            DateTime timestamp = message.RootElement
                    .GetProperty("header")
                    .GetProperty("gatewayTimestamp")
                    .GetDateTime()
                    .ToUniversalTime();

            // See https://github.com/EDCD/EDDN/blob/master/schemas/journal-v1.0.json for schema

            JsonElement messageElement = message.RootElement.GetProperty("message");
            string? eventType = null;
            string? starSystemName = null;
            string? systemSecurityState = null;
            EddnMinorFactionInfluence[] minorFactionInfos = Array.Empty<EddnMinorFactionInfluence>();
            EddnConflict[]? conflicts = Array.Empty<EddnConflict>();
            if (messageElement.TryGetProperty("event", out JsonElement eventProperty))
            {
                eventType = eventProperty.GetString();
                if (eventType == "Location" || eventType == "FSDJump" || eventType == "CarrierJump")
                {
                    if (messageElement.TryGetProperty("StarSystem", out JsonElement starSystemProperty))
                    {
                        starSystemName = starSystemProperty.GetString();
                    }
                    if (starSystemName != null
                        && messageElement.TryGetProperty("Factions", out JsonElement factionsProperty)
                        && factionsProperty.EnumerateArray().Any(element => minorFactionNameFilters.Matches(element.GetProperty("Name").GetString() ?? "")))
                    {
                        minorFactionInfos = factionsProperty.EnumerateArray().Select(element =>
                            new EddnMinorFactionInfluence()
                            {
                                MinorFaction = element.GetProperty("Name").GetString() ?? "",
                                Influence = element.GetProperty("Influence").GetDouble(),
                                States = element.TryGetProperty("ActiveStates", out JsonElement activeStatesElement)
                                    ? activeStatesElement.EnumerateArray().Select(stateElement => stateElement.GetProperty("State").GetString() ?? "").ToArray()
                                    : Array.Empty<string>()
                            }
                            ).ToArray();
                    }
                    if (messageElement.TryGetProperty("SystemSecurity", out JsonElement securityProperty))
                    {
                        systemSecurityState = securityProperty.GetString();
                    }
                    if (messageElement.TryGetProperty("Conflicts", out JsonElement conflictsProperty))
                    {
                        conflicts = conflictsProperty.Deserialize<EddnConflict[]>();
                    }
                }
            }

            return minorFactionInfos.Any()
                ? new EddnStarSystemData()
                {
                    Timestamp = timestamp,
                    StarSystemName = starSystemName ?? "",
                    MinorFactionDetails = minorFactionInfos,
                    SystemSecurityState = systemSecurityState ?? "",
                    Conflicts = conflicts ?? Array.Empty<EddnConflict>()
                } : null;
        }

        internal static void Update(OrderBotDbContext dbContext, EddnStarSystemData bgsSystemData)
        {
            StarSystem? starSystem = dbContext.StarSystems.FirstOrDefault(starSystem => starSystem.Name == bgsSystemData.StarSystemName);
            if (starSystem == null)
            {
                starSystem = new StarSystem { Name = bgsSystemData.StarSystemName, LastUpdated = bgsSystemData.Timestamp };
                dbContext.StarSystems.Add(starSystem);
            }
            else
            {
                starSystem.LastUpdated = bgsSystemData.Timestamp;
            }
            dbContext.SaveChanges();

            // Add or update minor factions
            foreach (EddnMinorFactionInfluence newMinorFactionInfo in bgsSystemData.MinorFactionDetails)
            {
                MinorFaction? minorFaction = dbContext.MinorFactions.FirstOrDefault(minorFaction => minorFaction.Name == newMinorFactionInfo.MinorFaction);
                if (minorFaction == null)
                {
                    minorFaction = new MinorFaction { Name = newMinorFactionInfo.MinorFaction };
                    dbContext.MinorFactions.Add(minorFaction);
                    dbContext.SaveChanges();
                }

                StarSystemMinorFaction? dbSystemMinorFaction = dbContext.StarSystemMinorFactions
                                                                        .Include(smf => smf.States)
                                                                        .Include(smf => smf.StarSystem)
                                                                        .Include(smf => smf.MinorFaction)
                                                                        .FirstOrDefault(
                                                                            smf => smf.StarSystem == starSystem
                                                                            && smf.MinorFaction == minorFaction);
                string? minorFactionSecurity =
                    newMinorFactionInfo == bgsSystemData.MinorFactionDetails.First(
                        mf => mf.Influence == bgsSystemData.MinorFactionDetails.Max(mf => mf.Influence))
                        ? bgsSystemData.SystemSecurityState : null;
                if (dbSystemMinorFaction == null)
                {
                    dbSystemMinorFaction = new StarSystemMinorFaction
                    {
                        MinorFaction = minorFaction,
                        StarSystem = starSystem,
                        Influence = newMinorFactionInfo.Influence,
                        SecurityLevel = minorFactionSecurity
                    };
                    dbContext.StarSystemMinorFactions.Add(dbSystemMinorFaction);
                }
                else
                {
                    dbSystemMinorFaction.Influence = newMinorFactionInfo.Influence;
                    dbSystemMinorFaction.SecurityLevel = minorFactionSecurity;
                }
                dbContext.SaveChanges();

                dbSystemMinorFaction.States.Clear();
                foreach (string stateName in newMinorFactionInfo.States)
                {
                    State? state = dbContext.States.FirstOrDefault(s => s.Name == stateName);
                    if (state == null)
                    {
                        state = new State { Name = stateName };
                        dbContext.States.Add(state);
                    }
                    dbSystemMinorFaction.States.Add(state);
                }
                dbContext.SaveChanges();
            }

            // Delete old minor factions
            SortedSet<string> newSystemMinorFactions = new(bgsSystemData.MinorFactionDetails.Select(mfd => mfd.MinorFaction));
            foreach (StarSystemMinorFaction systemMinorFaction in dbContext.StarSystemMinorFactions
                                                                           .Where(smf => smf.StarSystem == starSystem
                                                                                && !newSystemMinorFactions.Contains(smf.MinorFaction.Name)))
            {
                dbContext.StarSystemMinorFactions.Remove(systemMinorFaction);
            }

            // Save conflicts

            dbContext.SaveChanges();
        }
    }
}
