﻿using MinorFactionMonitor;
using NUnit.Framework;
using System.Text.Json;

namespace MinorFactionMonitor.Test
{
    public class TestEddnMessageProcessor
    {
        [Test]
        public void Ctor()
        {
            string[] minorFactions = new [] { "A", "B" };
            EddnMessageProcessor messageProcessor = new EddnMessageProcessor(minorFactions);

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
            EddnMessageProcessor messageProcessor = new EddnMessageProcessor(minorFactions);
            Assert.That(() => messageProcessor.GetTimestampAndFactionInfo(message), 
                expectedException != null ? Throws.InstanceOf(expectedException) : Throws.Nothing);
        }
    }
}