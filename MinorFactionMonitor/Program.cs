using Ionic.Zlib;
using NetMQ;
using NetMQ.Sockets;
using System.Text;
using System.Text.Json;

using SubscriberSocket client = new SubscriberSocket();
client.Options.ReceiveHighWatermark = 1000;
client.Connect("tcp://eddn.edcd.io:9500");
client.SubscribeToAnyTopic();

while (true)
{
    if (client.TryReceiveFrameBytes(out byte[]? compressed, out bool more))
    {
        byte[] uncompressed = ZlibStream.UncompressBuffer(compressed);
        string result = Encoding.UTF8.GetString(uncompressed);

        JsonDocument document = JsonDocument.Parse(result);

        // Get UTC time of last update
        DateTime timestamp = document.RootElement
                .GetProperty("header")
                .GetProperty("gatewayTimestamp")
                .GetDateTime();

        JsonElement messageElement = document.RootElement.GetProperty("message");
        string? starSystemName = null;
        if (messageElement.TryGetProperty("StarSystem", out JsonElement starSystemProperty))
        {
            starSystemName = starSystemProperty.GetString();
        }
        if (starSystemName != null && messageElement.TryGetProperty("Factions", out JsonElement factionsProperty))
        {
            factionsProperty.EnumerateArray().First(element => element.GetProperty("Name").GetString().Equals()
        }

        Console.WriteLine(result);
    }
}

record MinorFactionInfo
{
    string Name;
    double Influence;
    string[] States;
}

public 
