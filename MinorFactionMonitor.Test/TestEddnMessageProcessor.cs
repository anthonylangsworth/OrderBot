using MinorFactionMonitor;
using NUnit.Framework;
using System.Text.Json;

namespace MinorFactionMonitor.Test
{
    public class TestEddnMessageProcessor
    {
        [Test]
        public void Ctor()
        {
            NullLogger<EddnMessageProcessor> logger = new NullLogger<EddnMessageProcessor>();
            string[] minorFactions = new [] { "A", "B" };
            EddnMessageProcessor messageProcessor = new EddnMessageProcessor(logger, minorFactions);

            Assert.That(messageProcessor.Logger, Is.EqualTo(logger));
            Assert.That(messageProcessor.MinorFactions, Is.EquivalentTo(minorFactions));
        }

        [TestCase("", typeof(JsonException))]
        [TestCase("abcd", typeof(JsonException))]
        [TestCase("{}", typeof(KeyNotFoundException))]
        [TestCase("{\"header\":{}}", typeof(KeyNotFoundException))]
        [TestCase("{\"header\":{\"gatewayTimestamp\":\"\"}}", typeof(FormatException))]
        [TestCase("{\"header\":{\"gatewayTimestamp\":\"abc\"}}", typeof(FormatException))]
        public void ProcessMessageException(string message, Type? expectedException)
        {
            NullLogger<EddnMessageProcessor> logger = new NullLogger<EddnMessageProcessor>();
            string[] minorFactions = new[] { "A", "B" };
            EddnMessageProcessor messageProcessor = new EddnMessageProcessor(logger, minorFactions);
            Assert.That(() => messageProcessor.ProcessMessage(message), 
                expectedException != null ? Throws.InstanceOf(expectedException) : Throws.Nothing);
        }
    }
}