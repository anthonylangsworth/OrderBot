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
            HashSet<StarSystemMinorFaction> bgsData = new()
            {
                new StarSystemMinorFaction() { StarSystem = betelgeuse, MinorFaction = gumChewers, Influence = 0 },
                new StarSystemMinorFaction() { StarSystem = sirius, MinorFaction = gumChewers, Influence = 0 }
            };
            Assert.That(
                () => Goal.CheckAddActionsPreconditions(bgsData.First(), bgsData, new HashSet<Conflict>()),
                Throws.ArgumentException.And.Property("Message").EqualTo("All systemBgsData must be for star system Betelgeuse"));
        }

        [Test]
        public void CheckAddActionsPreconditions_MinorFactionNotInList()
        {
            StarSystem betelgeuse = new() { Name = "Betelgeuse" };
            MinorFaction gumChewers = new() { Name = "Gum Chewers" };
            MinorFaction funnyWalkers = new() { Name = "Funny Walkers" };
            MinorFaction bunnyHoppers = new() { Name = "Bunny Hoppoers" };
            HashSet<StarSystemMinorFaction> bgsData = new()
            {
                new StarSystemMinorFaction() { StarSystem = betelgeuse, MinorFaction = gumChewers, Influence = 0 },
                new StarSystemMinorFaction() { StarSystem = betelgeuse, MinorFaction = funnyWalkers, Influence = 0 }
            };
            StarSystemMinorFaction different = new() { StarSystem = betelgeuse, MinorFaction = bunnyHoppers, Influence = 0.5 };
            Assert.That(
                () => Goal.CheckAddActionsPreconditions(different, bgsData, new HashSet<Conflict>()),
                Throws.ArgumentException.And.Property("Message").EqualTo("systemBgsData must contain starSystemMinorFaction"));
        }

        [Test]
        public void CheckAddActionsPreconditions_MultipleConflicts()
        {
            StarSystem betelgeuse = new() { Name = "Betelgeuse" };
            StarSystem sirius = new() { Name = "Sirius" };
            MinorFaction gumChewers = new() { Name = "Gum Chewers" };
            MinorFaction funnyWalkers = new() { Name = "Funny Walkers" };
            MinorFaction bunnyHoppers = new() { Name = "Bunny Hoppoers" };
            MinorFaction sliders = new() { Name = "Sliders" };
            HashSet<StarSystemMinorFaction> bgsData = new()
            {
                new StarSystemMinorFaction() { StarSystem = betelgeuse, MinorFaction = gumChewers, Influence = 0 },
                new StarSystemMinorFaction() { StarSystem = betelgeuse, MinorFaction = funnyWalkers, Influence = 0 },
                new StarSystemMinorFaction() { StarSystem = betelgeuse, MinorFaction = sliders, Influence = 0 }
            };
            HashSet<Conflict> conflicts = new()
            {
                new Conflict() { StarSystem = betelgeuse, MinorFaction1 = bunnyHoppers, MinorFaction2 = gumChewers },
                new Conflict() { StarSystem = sirius, MinorFaction1 = funnyWalkers, MinorFaction2 = sliders  }
            };
            Assert.That(
                () => Goal.CheckAddActionsPreconditions(bgsData.First(), bgsData, conflicts),
                Throws.ArgumentException.And.Property("Message").EqualTo("All systemConflicts must be in star system Betelgeuse"));
        }

        [Test]
        public void CheckAddActionsPreconditions_ConflictMinorFactionNotInSystemBgsData()
        {
            StarSystem betelgeuse = new() { Name = "Betelgeuse" };
            StarSystem sirius = new() { Name = "Sirius" };
            MinorFaction gumChewers = new() { Name = "Gum Chewers" };
            MinorFaction funnyWalkers = new() { Name = "Funny Walkers" };
            MinorFaction bunnyHoppers = new() { Name = "Bunny Hoppoers" };
            MinorFaction sliders = new() { Name = "Sliders" };
            HashSet<StarSystemMinorFaction> bgsData = new()
            {
                new StarSystemMinorFaction() { StarSystem = betelgeuse, MinorFaction = gumChewers, Influence = 0 },
            };
            HashSet<Conflict> conflicts = new()
            {
                new Conflict() { StarSystem = betelgeuse, MinorFaction1 = bunnyHoppers, MinorFaction2 = gumChewers },
                new Conflict() { StarSystem = sirius, MinorFaction1 = funnyWalkers, MinorFaction2 = sliders  }
            };
            Assert.That(
                () => Goal.CheckAddActionsPreconditions(bgsData.First(), bgsData, conflicts),
                Throws.ArgumentException.And.Property("Message").EqualTo("All minor factions in systemConflicts must be in systemBgsData"));
        }

        [TestCaseSource(nameof(GetControllingMinorFaction_Source))]
        public StarSystemMinorFaction GetControllingMinorFaction(IReadOnlySet<StarSystemMinorFaction> systemBgsData)
        {
            return Goal.GetControllingMinorFaction(systemBgsData);
        }

        public static IEnumerable<TestCaseData> GetControllingMinorFaction_Source()
        {
            StarSystem betelgeuse = new() { Name = "Betelgeuse" };
            MinorFaction gumChewers = new() { Name = "Gum Chewers" };
            MinorFaction funnyWalkers = new() { Name = "Funny Walkers" };
            MinorFaction bunnyHoppers = new() { Name = "Bunny Hoppoers" };
            StarSystemMinorFaction gumChewersInBetegeuse = new() { StarSystem = betelgeuse, MinorFaction = gumChewers, Influence = 0.1 };
            StarSystemMinorFaction funnyWalkersInBetegeuse = new() { StarSystem = betelgeuse, MinorFaction = funnyWalkers, Influence = 0.3 };
            StarSystemMinorFaction bunnyHoppersInBetegeuse = new() { StarSystem = betelgeuse, MinorFaction = bunnyHoppers, Influence = 0.5 };

            return new TestCaseData[]
            {
                new TestCaseData(new HashSet<StarSystemMinorFaction>()
                {
                    gumChewersInBetegeuse
                }).Returns(gumChewersInBetegeuse),
                new TestCaseData(new HashSet<StarSystemMinorFaction>()
                {
                    gumChewersInBetegeuse,
                    funnyWalkersInBetegeuse
                }).Returns(funnyWalkersInBetegeuse),
                new TestCaseData(new HashSet<StarSystemMinorFaction>()
                {
                    gumChewersInBetegeuse,
                    funnyWalkersInBetegeuse,
                    bunnyHoppersInBetegeuse
                }).Returns(bunnyHoppersInBetegeuse)
            };
        }
    }
}
