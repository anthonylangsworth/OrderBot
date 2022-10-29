using NUnit.Framework;
using OrderBot.Core;
using OrderBot.ToDo;

namespace OrderBot.Test.ToDo
{
    internal class TestGoal
    {
        [Test]
        public void CheckAddActionsPreconditions_MultipleSystems()
        {
            StarSystem betelgeuse = new() { Name = "Betelgeuse" };
            StarSystem sirius = new() { Name = "Sirius" };
            MinorFaction gumChewers = new() { Name = "Gum Chewers" };
            StarSystemMinorFaction[] bgsData = new[]
            {
                new StarSystemMinorFaction() { StarSystem = betelgeuse, MinorFaction = gumChewers, Influence = 0 },
                new StarSystemMinorFaction() { StarSystem = sirius, MinorFaction = gumChewers, Influence = 0 }
            };
            Assert.That(
                () => Goal.CheckAddActionsPreconditions(bgsData[0], bgsData),
                Throws.ArgumentException.And.Property("Message").EqualTo("systemBgsData must contain data for one star system"));
        }

        [Test]
        public void CheckAddActionsPreconditions_MinorFactionNotInList()
        {
            StarSystem betelgeuse = new() { Name = "Betelgeuse" };
            MinorFaction gumChewers = new() { Name = "Gum Chewers" };
            MinorFaction funnyWalkers = new() { Name = "Funny Walkers" };
            MinorFaction bunnyHoppers = new() { Name = "Bunny Hoppoers" };
            StarSystemMinorFaction[] bgsData = new[]
            {
                new StarSystemMinorFaction() { StarSystem = betelgeuse, MinorFaction = gumChewers, Influence = 0 },
                new StarSystemMinorFaction() { StarSystem = betelgeuse, MinorFaction = funnyWalkers, Influence = 0 }
            };
            StarSystemMinorFaction different = new() { StarSystem = betelgeuse, MinorFaction = bunnyHoppers, Influence = 0.5 };
            Assert.That(
                () => Goal.CheckAddActionsPreconditions(different, bgsData),
                Throws.ArgumentException.And.Property("Message").EqualTo("systemBgsData must contain starSystemMinorFaction"));
        }
    }
}
