using NUnit.Framework;
using OrderBot.MessageProcessors;
using System.Text.Json;

namespace OrderBot.Test.MessageProcessors;
internal class EddnMessageHostedServiceTests
{
    [Test]
    [TestCaseSource(nameof(GetGameVersion_Source))]
    public Version? GetGameVersion(string message)
    {
        return EddnMessageHostedService.GetGameVersion(JsonDocument.Parse(message));
    }

    public static IEnumerable<TestCaseData> GetGameVersion_Source()
    {
        return new TestCaseData[]
        {
            new TestCaseData("{\"header\": {}}").Returns(null),
            new TestCaseData("{\"header\": { \"gameversion\": \"\"}}").Returns(null),
            new TestCaseData("{\"header\": { \"gameversion\": \"4.0\"}}").Returns(new Version(4, 0)),
            new TestCaseData("{\"header\": { \"gameversion\": \"4.1\"}}").Returns(new Version(4, 1)),
            new TestCaseData("{\"header\": { \"gameversion\": \"4.0.0.1475\"}}").Returns(new Version(4, 0, 0, 1475))
        };
    }

    [Test]
    public void RequiredMinimumVersion()
    {
        Assert.That(EddnMessageHostedService.RequiredGameVersion, Is.EqualTo(new Version(4, 0)));
    }
}
