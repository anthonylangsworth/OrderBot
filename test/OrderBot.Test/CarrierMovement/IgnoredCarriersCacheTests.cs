using NUnit.Framework;
using OrderBot.CarrierMovement;
using OrderBot.Core;
using OrderBot.Test.Samples;

namespace OrderBot.Test.CarrierMovement;
internal class IgnoredCarriersCachTests : CacheTest<IgnoredCarriersCache>
{
    public IgnoredCarriersCachTests()
        : base(memoryCache => new IgnoredCarriersCache(memoryCache))
    {
        // Do nothing
    }

    [Test]
    [TestCase(1UL, "123-456", ExpectedResult = false)]
    public bool IsIgnored_None(ulong discordGuidId, string carrierSerialNumber)
    {
        return Cache.IsIgnored(DbContext, discordGuidId, carrierSerialNumber);
    }

    [Test]
    [TestCase(1UL, CarrierSerialNumbers.PriorityZero, ExpectedResult = true)]
    [TestCase(1UL, CarrierSerialNumbers.Invincible, ExpectedResult = true)]
    [TestCase(1UL, CarrierSerialNumbers.MyOtherShipIsAThargoid, ExpectedResult = false)]

    [TestCase(2UL, CarrierSerialNumbers.PriorityZero, ExpectedResult = false)]
    [TestCase(2UL, CarrierSerialNumbers.Invincible, ExpectedResult = true)]
    [TestCase(2UL, CarrierSerialNumbers.MyOtherShipIsAThargoid, ExpectedResult = true)]

    [TestCase(3UL, CarrierSerialNumbers.PriorityZero, ExpectedResult = false)]
    [TestCase(3UL, CarrierSerialNumbers.Invincible, ExpectedResult = false)]
    [TestCase(3UL, CarrierSerialNumbers.MyOtherShipIsAThargoid, ExpectedResult = false)]

    [TestCase(4UL, CarrierSerialNumbers.PriorityZero, ExpectedResult = false)]
    [TestCase(4UL, CarrierSerialNumbers.Invincible, ExpectedResult = false)]
    [TestCase(4UL, CarrierSerialNumbers.MyOtherShipIsAThargoid, ExpectedResult = false)]
    public bool IsIgnored(ulong discordGuidId, string carrierSerialNumber)
    {
        Carrier priorityZero = new() { Name = CarrierNames.PriorityZero };
        Carrier invincible = new() { Name = CarrierNames.Invincible };
        Carrier myOtherShipIsAThargoid = new() { Name = CarrierNames.MyOtherShipIsAThargoid };
        DbContext.Carriers.AddRange(priorityZero, invincible, myOtherShipIsAThargoid);
        DbContext.SaveChanges();

        DiscordGuild discordGuild1 = new() { GuildId = 1 };
        DiscordGuild discordGuild2 = new() { GuildId = 2 };
        DiscordGuild discordGuild3 = new() { GuildId = 3 };
        DbContext.DiscordGuilds.AddRange(discordGuild1, discordGuild2, discordGuild3);
        DbContext.SaveChanges();

        discordGuild1.IgnoredCarriers.Add(priorityZero);
        discordGuild1.IgnoredCarriers.Add(invincible);
        discordGuild2.IgnoredCarriers.Add(myOtherShipIsAThargoid);
        discordGuild2.IgnoredCarriers.Add(invincible);
        DbContext.SaveChanges();

        return Cache.IsIgnored(DbContext, discordGuidId, carrierSerialNumber);
    }
}
