using NUnit.Framework;
using OrderBot.CarrierMovement;
using OrderBot.Core;
using OrderBot.Test.Samples;

namespace OrderBot.Test.CarrierMovement;

internal class CarriersAutocompleteHandlersTests : DbTest
{
    internal DiscordGuild Guild { get; set; } = null!;
    internal static IReadOnlyList<string> Carriers { get; set; } = new string[]
    {
        CarrierNames.HighGradeEmissions,
        CarrierNames.Indivisible,
        CarrierNames.Invincible,
        CarrierNames.Invisible,
        CarrierNames.MyOtherShipIsAThargoid,
        CarrierNames.PizzaDeliveryVan,
        CarrierNames.PriorityZero
    }.OrderBy(c => c)
     .ToList();

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();

        Guild = new DiscordGuild() { GuildId = 1234567890, Name = "Test Guild" };
        foreach (string carrierName in Carriers)
        {
            Carrier carrier = new() { Name = carrierName };
            DbContext.Carriers.Add(carrier);
            Guild.IgnoredCarriers.Add(carrier);
        }
        DbContext.DiscordGuilds.Add(Guild);
        DbContext.SaveChanges();
    }

    [Test]
    [TestCaseSource(nameof(NotIgnored_GetCarriers_Source))]
    public IEnumerable<string> NotIgnored_GetCarriers(string nameStartsWith, IEnumerable<string> ignoredCarriers)
    {
        Guild.IgnoredCarriers.Clear();
        foreach (string carrierName in ignoredCarriers)
        {
            Guild.IgnoredCarriers.Add(DbContext.Carriers.First(c => c.Name == carrierName));
        }
        DbContext.SaveChanges();

        return new NotIgnoredCarriersAutocompleteHandler(DbContext).GetCarriers(DbContext, Guild, nameStartsWith);
    }

    public static IEnumerable<TestCaseData> NotIgnored_GetCarriers_Source()
    {
        return new (string NameStartsWith, string[] IgnoredCarriers)[]
        {
            ("", Array.Empty<string>()),
            ("", new string[]
                {
                    CarrierNames.HighGradeEmissions
                }),
            ("a", new string[]
                {
                    CarrierNames.HighGradeEmissions
                }),
            ("pr", new string[]
                {
                    CarrierNames.HighGradeEmissions
                }),
            ("p", new string[]
                {
                    CarrierNames.PriorityZero
                }),
            ("p", new string[]
                {
                    CarrierNames.PizzaDeliveryVan,
                    CarrierNames.PriorityZero
                }),
            ("U.S.S. ", new string[]
                {
                    CarrierNames.PizzaDeliveryVan,
                    CarrierNames.PriorityZero
                })
        }.Select(tuple => new TestCaseData(tuple.NameStartsWith, tuple.IgnoredCarriers)
                              .Returns(Carriers.Where(c => c.StartsWith(tuple.NameStartsWith, StringComparison.OrdinalIgnoreCase)
                                                        && !tuple.IgnoredCarriers.Contains(c))));
    }

    [Test]
    [TestCaseSource(nameof(Ignored_GetCarriers_Source))]
    public IEnumerable<string> Ignored_GetCarriers(string nameStartsWith, IEnumerable<string> ignoredCarriers)
    {
        Guild.IgnoredCarriers.Clear();
        foreach (string carrierName in ignoredCarriers)
        {
            Guild.IgnoredCarriers.Add(DbContext.Carriers.First(c => c.Name == carrierName));
        }
        DbContext.SaveChanges();

        return new IgnoredCarriersAutocompleteHandler(DbContext).GetCarriers(DbContext, Guild, nameStartsWith);
    }

    public static IEnumerable<TestCaseData> Ignored_GetCarriers_Source()
    {
        return new (string NameStartsWith, string[] IgnoredCarriers)[]
        {
            ("", Array.Empty<string>()),
            ("", new string[]
                {
                    CarrierNames.HighGradeEmissions
                }),
            ("a", new string[]
                {
                    CarrierNames.HighGradeEmissions
                }),
            ("pr", new string[]
                {
                    CarrierNames.HighGradeEmissions
                }),
            ("p", new string[]
                {
                    CarrierNames.PriorityZero
                }),
            ("p", new string[]
                {
                    CarrierNames.PizzaDeliveryVan,
                    CarrierNames.PriorityZero
                }),
            ("U.S.S. ", new string[]
                {
                    CarrierNames.PizzaDeliveryVan,
                    CarrierNames.PriorityZero
                })
        }.Select(tuple => new TestCaseData(tuple.NameStartsWith, tuple.IgnoredCarriers)
                              .Returns(Carriers.Where(c => c.StartsWith(tuple.NameStartsWith, StringComparison.OrdinalIgnoreCase)
                                                        && tuple.IgnoredCarriers.Contains(c))));
    }

}
