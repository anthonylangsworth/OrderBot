using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MinorFactionMonitor
{
    internal class EddnMessageProcessor
    {
        public EddnMessageProcessor(ILogger<EddnMessageProcessor> logger, IEnumerable<string> minorFactions)
        {
            Logger = logger;
            MinorFactions = new HashSet<string>(minorFactions);

            logger.LogInformation(
                "{Type} started. Monitoring minor factions: {MinorFactions}", 
                GetType().Name, string.Join(",", minorFactions));
        }

        public ILogger<EddnMessageProcessor> Logger { get; }
        public ISet<string> MinorFactions { get; }

        public void ProcessMessage(string message)
        {
            JsonDocument document = JsonDocument.Parse(message);
            DateTime timestamp = document.RootElement
                    .GetProperty("header")
                    .GetProperty("gatewayTimestamp")
                    .GetDateTime();

            JsonElement messageElement = document.RootElement.GetProperty("message");
            string? starSystemName = null;
            IEnumerable<MinorFactionInfo> minorFactions;
            if (messageElement.TryGetProperty("StarSystem", out JsonElement starSystemProperty))
            {
                starSystemName = starSystemProperty.GetString();
            }
            if (starSystemName != null
                && messageElement.TryGetProperty("Factions", out JsonElement factionsProperty)
                && factionsProperty.EnumerateArray().Any(element => MinorFactions.Contains(element.GetProperty("Name").GetString() ?? "")))
            {
                minorFactions = factionsProperty.EnumerateArray().Select(element =>
                    new MinorFactionInfo(
                        element.GetProperty("Name").GetString() ?? "",
                        element.GetProperty("Influence").GetDouble(),
                        element.TryGetProperty("ActiveStates", out JsonElement activeStatesElement) 
                            ? activeStatesElement.EnumerateArray().Select(stateElement => stateElement.GetProperty("State").GetString() ?? "").ToArray()
                            : Array.Empty<string>()
                    ));

                Console.WriteLine(string.Join(",", minorFactions));
            }
        }
    }
}
