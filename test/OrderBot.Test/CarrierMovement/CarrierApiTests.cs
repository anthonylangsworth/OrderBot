using Discord;
using Moq;
using NUnit.Framework;
using OrderBot.CarrierMovement;
using OrderBot.Test.Samples;

namespace OrderBot.Test.CarrierMovement;
internal class CarrierApiTests : DbTest
{
    public void Integration()
    {
        const ulong testGuildId = 1234567890;
        const string testGuildName = "My Discord Server";
        IGuild guild = Mock.Of<IGuild>(g => g.Id == testGuildId && g.Name == testGuildName);
        CarrierApi api = new(DbContext, guild);

        Assert.That(api.ListIgnoredCarriers(), Is.Empty);

        // Add single carrier
        api.AddIgnoredCarriers(new[] { CarrierNames.Invincible });
        Assert.That(api.ListIgnoredCarriers(), Is.EquivalentTo(new[] { CarrierNames.Invincible }));

        // Add same carreir
        api.AddIgnoredCarriers(new[] { CarrierNames.Invincible });
        Assert.That(api.ListIgnoredCarriers(), Is.EquivalentTo(new[] { CarrierNames.Invincible }));

        // Add different carrier
        api.AddIgnoredCarriers(new[] { CarrierNames.PriorityZero });
        Assert.That(api.ListIgnoredCarriers(), Is.EquivalentTo(new[] { CarrierNames.Invincible, CarrierNames.PriorityZero }));

        // Remove one carrier
        api.RemoveIgnoredCarrier(CarrierNames.Invincible);
        Assert.That(api.ListIgnoredCarriers(), Is.EquivalentTo(new[] { CarrierNames.PriorityZero }));

        // Remove non-ignored carrier
        api.RemoveIgnoredCarrier(CarrierNames.PizzaDeliveryVan);
        Assert.That(api.ListIgnoredCarriers(), Is.EquivalentTo(new[] { CarrierNames.PriorityZero }));

        // Add it back
        api.AddIgnoredCarriers(new[] { CarrierNames.Invincible });
        Assert.That(api.ListIgnoredCarriers(), Is.EquivalentTo(new[] { CarrierNames.Invincible, CarrierNames.PriorityZero }));

        api.RemoveIgnoredCarrier(CarrierNames.PriorityZero);
        api.RemoveIgnoredCarrier(CarrierNames.Invincible);
        Assert.That(api.ListIgnoredCarriers(), Is.Empty);
    }
}
