using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using OrderBot.Core;
using OrderBot.EntityFramework;
using System.Transactions;

namespace OrderBot.Test.Core
{
    internal class TestCarrier
    {
        [Test]
        [TestCase("", ExpectedResult = false)]
        [TestCase("a", ExpectedResult = false)]
        [TestCase("ab", ExpectedResult = false)]
        [TestCase("abc-def", ExpectedResult = true)]
        [TestCase("bc-def", ExpectedResult = false)]
        [TestCase("abc-defg", ExpectedResult = false)]
        [TestCase("Ship a1c-de2", ExpectedResult = true)]
        public bool IsCarrier(string signalName)
        {
            return Carrier.IsCarrier(signalName);
        }

        [Test]
        [TestCase("", true)]
        [TestCase("a", true)]
        [TestCase("ab", true)]
        [TestCase("abc-def", false)]
        [TestCase("bc-def", true)]
        [TestCase("abc-defg", true)]
        [TestCase("Ship a1c-de2", false)]
        public void Ctor(string name, bool throws)
        {
            if (throws)
            {
                Assert.That(() => new Carrier() { Name = name }, Throws.ArgumentException);
            }
            else
            {
                Carrier carrier = new() { Name = name };
                Assert.That(carrier.Name, Is.EqualTo(name));
            }
        }

        [Test]
        [TestCase("abc-def", ExpectedResult = "abc-def")]
        [TestCase("Ship a1c-de2", ExpectedResult = "a1c-de2")]
        public string GetSerialNumber(string signalName)
        {
            return Carrier.GetSerialNumber(signalName);
        }

        [Test]
        [Ignore("Only run manually")]
        public void IgnoreEDACarriers()
        {
            ulong edaGuildId = 141831692699566080;
            // Source: https://inara.cz/elite/squadron-assets/687/ and in-game
            string[] carriers =
            {
                "E.D.A. SANDGROPER YNX-82Z",
                "E.D.A. Amphion Q3H-7HT",
                "E.D.A. KOSCIUSKO V7Q-N6H",
                "E.D.A. HAMPSTER WHEEL X3B-8QF",
                "E.D.A RAINBOW SERPeNT HZT-L9V",
                "E.D.A. MATES RATES JLX-N8B",
                "E.D.A. BEECHWORTH K0Z-63N",
                "T.N.V.A. SHINANO XLJ-L5Q",
                "E.D.A BUNNINGS SNAG X4Z-7QH",
                "E.D.A. MITHLOND J9B-0QX",
                "E.D.A. CERRITOS Q8Y-78T",
                "E.D.A. WAIKATO T9J-8XK",
                "E.D.A SUPERNOVA XLH-02Y",
                "E.D.A CRUSTY STICK J5B-L9Q",
                "E.D.A. BLACK PRINCE T9Z-3VT",
                "E.D.A. HENRY CAVILL V2G-G8F",
                "E.D.A. OBERON CRUSADER V4T-7QV",
                "E.D.A. CERRITOS Q8Y-78T",
                "E.D.A. HENRY CAVILL V2G-G8F",
                "THE GRID X3T-92T",
                "E.D.A AURORA AUSTRALIS V1B-L3B",
                "E.D.A. HIGHBURY H8N-40Q",
                "E.D.A HOVER CARGO HZH-4TL",
                "E.D.A.Wizard's Rage F1Q-3QZ",
                "E.D.A. Ned Kelly Q2B-27T",
                "Eda Zz9 Plural Z Alpha K5K-L5T",
                "E.D.A. Russell Coight QHK-72B",
                "New Zealand KNM-8VZ",
                "E.D.A Merv Hughes V8J-94L",
                "E.D.A. Corroboree V1J-74J",
                "E.D.A Sense8 H1K-8VZ",
                "E.D.A. First Strike V5Y-7TK",
                "EDA The Eve Of The War V6K-34Y",
                "E.D.A. The Pool Room X5W-BVY",
                "E.D.A. Sheep In Boots HLN-9AZ",
                "E.D.A. Relativity V4T-58N",
                "E.D.A Devastator HBG-T4L",
                "Chaos Under Heaven XHB-4QW",
                "E.D.A RAINBOW SERPeNT HZT-L9V",
                "e.d.a pride of hiigara V4K-B5W",
                "E.D.A Supernova XLH-02Y",
                "E.D.A. Bin Chicken X9V-6KJ",
                "E.D.A. Cantankerous N0F-20Z",
                "E.D.A. Chimaera M5L-GVZ",
                "E.D.A. Codfish Island HNW-W1Z",
                "E.D.A. Don Kabana X4L-80V",
                "E.D.A. Kosciusko V7Q-N6H",
                "E.D.A. Walkabout KHF-79Z",
                "Eda - Inevitable Decay Q6H-T6F",
                "Eda Siaubas Baubas T0K-1QN",
                "Eda-Galactic Hobo X5Z-92V",
                "Hmass Horrible Carrier V0T-83W",
                "Infinite Regression T7Z-18M",
                "Kiss Me Already K1H-B1V",
                "Naughtius Maximus Q9Y-TQZ",
                "Orion's Legacy V9T-T4B",
                "selling wine T5H-T9K",
                "Shadow Of Alduin V9K-52M",
                "The ******Maximum K3W-L9V",
                "The Dogs Kennel K8B-NQZ",
                "Where Am I Now Q0K-9QN",
                "Your-Grandma X0J-N7B",
                "BUTT STALLION GHT-0QZ",
                "E.D.A. Frank Walker V6T-4TZ",
                "HMNZS RESOLUTE XHT-98W"
            };

            using OrderBotDbContextFactory orderBotDbContextFactory = new();
            using OrderBotDbContext dbContext = orderBotDbContextFactory.CreateDbContext();
            using TransactionScope transactionScope = new();
            DiscordGuild? discordGuild = dbContext.DiscordGuilds.Include(dg => dg.IgnoredCarriers)
                                                                .FirstOrDefault(dg => dg.GuildId == edaGuildId);
            if (discordGuild == null)
            {
                Assert.Fail("No guild found");
            }
            else
            {
                foreach (string carrierName in carriers.Select(s => s.ToUpper().Trim()))
                {
                    Carrier? carrier = dbContext.Carriers.FirstOrDefault(c => c.SerialNumber == Carrier.GetSerialNumber(carrierName));
                    if (carrier == null)
                    {
                        carrier = new Carrier() { Name = carrierName };
                        dbContext.Carriers.Add(carrier);
                    }

                    if (!discordGuild.IgnoredCarriers.Contains(carrier))
                    {
                        discordGuild.IgnoredCarriers.Add(carrier);
                    }
                }
            }
            dbContext.SaveChanges();
            transactionScope.Complete();
        }
    }
}
