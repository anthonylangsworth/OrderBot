using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderBot.Core;
using System.Data;
using System.Text.Json;
using System.Transactions;

namespace OrderBot.MessageProcessors
{
    internal class SystemMinorFactionMessageProcessor : EddnMessageProcessor
    {
        public SystemMinorFactionMessageProcessor(ILogger<SystemMinorFactionMessageProcessor> logger, IDbContextFactory<OrderBotDbContext> dbContextFactory, MinorFactionsSource minorFactionsSource)
        {
            Logger = logger;
            DbContextFactory = dbContextFactory;
            MinorFactionsSource = minorFactionsSource;
        }

        public ILogger<SystemMinorFactionMessageProcessor> Logger { get; }
        public IDbContextFactory<OrderBotDbContext> DbContextFactory { get; }
        public MinorFactionsSource MinorFactionsSource { get; }

        public override void Process(string message)
        {
            using (OrderBotDbContext dbContext = DbContextFactory.CreateDbContext())
            using (TransactionScope transactionScope = new())
            {
                (DateTime timestamp, string? starSystemName, MinorFactionInfo[] minorFactionDetails) =
                    GetTimestampAndFactionInfo(message, MinorFactionsSource.Get());
                if (starSystemName != null && minorFactionDetails.Length > 0)
                {
                    //IExecutionStrategy executionStrategy = dbContext.Database.CreateExecutionStrategy();
                    //executionStrategy.Execute(() => InnerSink(timestamp, starSystemName, minorFactionDetails, dbContext));
                    Update(timestamp, starSystemName, minorFactionDetails, dbContext);
                    transactionScope.Complete();

                    Logger.LogInformation("System {system} updated", starSystemName);
                }
            }
        }

        /// <summary>
        /// Extract the timestamp and info for relevant minor factions.
        /// </summary>
        /// <param name="message">
        /// The message received from EDDN.
        /// </param>
        /// <param name="minorFactions">
        /// The minor factions to filter by.
        /// </param>
        /// <returns>
        /// The message's UTC timestamp and an array of <see cref="MinorFactionInfo"/> with relevant
        /// details about the system. If this array is empty, there are no relevant details.
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
        internal static (DateTime, string?, MinorFactionInfo[]) GetTimestampAndFactionInfo(string message, IReadOnlySet<string> minorFactions)
        {
            JsonDocument document = JsonDocument.Parse(message);
            DateTime timestamp = document.RootElement
                    .GetProperty("header")
                    .GetProperty("gatewayTimestamp")
                    .GetDateTime()
                    .ToUniversalTime();

            JsonElement messageElement = document.RootElement.GetProperty("message");
            string? starSystemName = null;
            MinorFactionInfo[] minorFactionInfos = new MinorFactionInfo[0];
            if (messageElement.TryGetProperty("StarSystem", out JsonElement starSystemProperty))
            {
                starSystemName = starSystemProperty.GetString();
            }
            if (starSystemName != null
                && messageElement.TryGetProperty("Factions", out JsonElement factionsProperty)
                && factionsProperty.EnumerateArray().Any(element => minorFactions.Contains(element.GetProperty("Name").GetString() ?? "")))
            {
                minorFactionInfos = factionsProperty.EnumerateArray().Select(element =>
                    new MinorFactionInfo(
                        element.GetProperty("Name").GetString() ?? "",
                        element.GetProperty("Influence").GetDouble(),
                        element.TryGetProperty("ActiveStates", out JsonElement activeStatesElement)
                            ? activeStatesElement.EnumerateArray().Select(stateElement => stateElement.GetProperty("State").GetString() ?? "").ToArray()
                            : Array.Empty<string>()
                    )).ToArray();
            }

            return (timestamp, starSystemName, minorFactionInfos);
        }

        internal static void Update(DateTime timestamp, string starSystemName, IEnumerable<MinorFactionInfo> minorFactionDetails, OrderBotDbContext dbContext)
        {
            StarSystem? starSystem = dbContext.StarSystems.FirstOrDefault(starSystem => starSystem.Name == starSystemName);
            if (starSystem == null)
            {
                starSystem = new StarSystem { Name = starSystemName, LastUpdated = timestamp };
                dbContext.StarSystems.Add(starSystem);
            }
            else
            {
                starSystem.LastUpdated = timestamp;
            }
            dbContext.SaveChanges();

            // Add or update minor factions
            foreach (MinorFactionInfo newMinorFactionInfo in minorFactionDetails)
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
                if (dbSystemMinorFaction == null)
                {
                    dbSystemMinorFaction = new StarSystemMinorFaction
                    {
                        MinorFaction = minorFaction,
                        StarSystem = starSystem,
                        Influence = newMinorFactionInfo.Influence,
                    };
                    dbContext.StarSystemMinorFactions.Add(dbSystemMinorFaction);
                }
                else
                {
                    dbSystemMinorFaction.Influence = newMinorFactionInfo.Influence;
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
            SortedSet<string> newSystemMinorFactions = new(minorFactionDetails.Select(mfd => mfd.MinorFaction));
            foreach (StarSystemMinorFaction systemMinorFaction in dbContext.StarSystemMinorFactions
                                                                           .Where(smf => smf.StarSystem == starSystem
                                                                                && !newSystemMinorFactions.Contains(smf.MinorFaction.Name)))
            {
                dbContext.StarSystemMinorFactions.Remove(systemMinorFaction);
            }
            dbContext.SaveChanges();
        }
    }
}
