using NUnit.Framework;
using System.Text.Json;

namespace OrderBot.MessageProcessors.Test
{
    public class TestEddnMessageExtractor
    {
        [Test]
        public void Ctor()
        {
            string[] minorFactions = new[] { "A", "B" };
            EddnMessageExtractor messageProcessor = new EddnMessageExtractor(minorFactions);

            Assert.That(messageProcessor.MinorFactions, Is.EquivalentTo(minorFactions));
        }

        [TestCase("", typeof(JsonException))]
        [TestCase("abcd", typeof(JsonException))]
        [TestCase("{}", typeof(KeyNotFoundException))]
        [TestCase("{\"header\":{}}", typeof(KeyNotFoundException))]
        [TestCase("{\"header\":{\"gatewayTimestamp\":\"\"}}", typeof(FormatException))]
        [TestCase("{\"header\":{\"gatewayTimestamp\":\"abc\"}}", typeof(FormatException))]
        public void GetTimestampAndFactionInfoException(string message, Type? expectedException)
        {
            string[] minorFactions = new[] { "A", "B" };
            EddnMessageExtractor messageProcessor = new EddnMessageExtractor(minorFactions);
            Assert.That(() => messageProcessor.GetTimestampAndFactionInfo(message),
                expectedException != null ? Throws.InstanceOf(expectedException) : Throws.Nothing);
        }

        public static object[] GetTimeStampAndFactionInfoSource() => new[]
        {
            new object []{ "hausersReach.json", Array.Empty<string>(), "2022-02-24T12:09:53.335118Z", "Robigo", Array.Empty<MinorFactionInfo>() },
            new object []{ "hausersReach.json", new [] { "A" }, "2022-02-24T12:09:53.335118Z", "Robigo", Array.Empty<MinorFactionInfo>() },
            new object []{ "hausersReach.json", new [] { "Sirius Corporation" }, "2022-02-24T12:09:53.335118Z", "Robigo", new MinorFactionInfo[] {
                    new MinorFactionInfo("Sirius Corporation", 0.567677, new [] { "Boom" } ),
                    new MinorFactionInfo("Robigo Cartel", 0.206061, new [] { "InfrastructureFailure", "CivilWar" }),
                    new MinorFactionInfo("CdE Corporation", 0.226263, new [] { "CivilWar" })
                }
            }
        };

        [TestCaseSource(nameof(GetTimeStampAndFactionInfoSource))]
        public void GetTimestampAndFactionInfo(string fileName, string[] minorFactions, string expectedTimestamp, string? expectedSystemName, MinorFactionInfo[] expectedMinorFactionInfo)
        {
            EddnMessageExtractor messageProcessor = new EddnMessageExtractor(minorFactions);
            using StreamReader streamReader = File.OpenText($"samples/{fileName}");
            (DateTime timestamp, string? systemName, MinorFactionInfo[] minorFactionInfo) = messageProcessor.GetTimestampAndFactionInfo(streamReader.ReadToEnd());
            Assert.That(timestamp, Is.EqualTo(DateTime.Parse(expectedTimestamp).ToUniversalTime()));
            Assert.That(systemName, Is.EqualTo(expectedSystemName));
            Assert.That(minorFactionInfo, Is.EquivalentTo(expectedMinorFactionInfo));
        }
    }
}