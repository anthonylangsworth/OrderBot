using Ionic.Zlib;
using NetMQ;
using NetMQ.Sockets;
using System.Text;

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
        Console.WriteLine(result);
    }
}
