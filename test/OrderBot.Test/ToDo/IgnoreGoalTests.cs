using NUnit.Framework;
using OrderBot.Core;
using OrderBot.ToDo;

namespace OrderBot.Test.ToDo
{
    internal class IgnoreGoalTests
    {
        [Test]
        public void Instance()
        {
            Assert.That(IgnoreGoal.Instance.Name, Is.EqualTo("Ignore"));
            Assert.That(IgnoreGoal.Instance.Description, Is.EqualTo("Never suggest activity."));
        }

        [Test]
        [TestCaseSource(nameof(AddActions_Source))]
        public void AddActions(Presence starSystemMinorFaction, IReadOnlySet<Presence> systemPresences,
            IReadOnlySet<Conflict> systemConflicts)
        {
            Assert.That(
                IgnoreGoal.Instance.GetSuggestions(starSystemMinorFaction, systemPresences, systemConflicts),
                Is.Empty);
        }

        public static IEnumerable<TestCaseData> AddActions_Source()
        {
            StarSystem polaris = new() { Name = "Polaris", LastUpdated = DateTime.UtcNow };
            MinorFaction flyingFish = new() { Name = "Flying Fish" };
            MinorFaction bloatedJellyFish = new() { Name = "Bloated Jelly Fish" };
            Presence lowInfluence = new()
            {
                StarSystem = polaris,
                MinorFaction = flyingFish,
                Influence = 0.1,
                SecurityLevel = SecurityLevel.High
            };
            Presence mediumInfluence = new()
            {
                StarSystem = polaris,
                MinorFaction = flyingFish,
                Influence = 0.5,
                SecurityLevel = SecurityLevel.Medium
            };
            Presence highInfluence = new()
            {
                StarSystem = polaris,
                MinorFaction = flyingFish,
                Influence = 0.9,
                SecurityLevel = SecurityLevel.Low
            };
            Presence bloatedJellyFishInPolaris = new()
            {
                StarSystem = polaris,
                MinorFaction = bloatedJellyFish,
                Influence = ControlGoal.UpperInfluenceThreshold,
                SecurityLevel = null
            };
            Conflict war = new()
            {
                StarSystem = polaris,
                MinorFaction1 = flyingFish,
                MinorFaction1WonDays = 2,
                MinorFaction2 = bloatedJellyFish,
                MinorFaction2WonDays = 1,
                WarType = WarType.War,
                Status = ConflictStatus.Active
            };
            Conflict civilWar = new()
            {
                StarSystem = polaris,
                MinorFaction1 = bloatedJellyFish,
                MinorFaction1WonDays = 0,
                MinorFaction2 = flyingFish,
                MinorFaction2WonDays = 3,
                WarType = WarType.CivilWar,
                Status = ConflictStatus.Active
            };
            Conflict election = new()
            {
                StarSystem = polaris,
                MinorFaction1 = bloatedJellyFish,
                MinorFaction1WonDays = 2,
                MinorFaction2 = flyingFish,
                MinorFaction2WonDays = 1,
                WarType = WarType.Election,
                Status = ConflictStatus.Active
            };

            return new[]
            {
                new TestCaseData(
                    lowInfluence,
                    new HashSet<Presence>() { lowInfluence },
                    new HashSet<Conflict>()
                ).SetName("AddActions Low"),
                new TestCaseData(
                    mediumInfluence,
                    new HashSet<Presence> { mediumInfluence },
                    new HashSet<Conflict>()
                ).SetName("AddActions Medium"),
                new TestCaseData(
                    highInfluence,
                    new HashSet<Presence> { highInfluence },
                    new HashSet<Conflict>()
                ).SetName("AddActions High"),
                new TestCaseData(
                    lowInfluence,
                    new HashSet<Presence>() { lowInfluence, bloatedJellyFishInPolaris },
                    new HashSet<Conflict>() { war }
                ).SetName("AddActions War"),
                new TestCaseData(
                    lowInfluence,
                    new HashSet<Presence>() { lowInfluence, bloatedJellyFishInPolaris },
                    new HashSet<Conflict>() { civilWar }
                ).SetName("AddActions CivilWar"),
                new TestCaseData(
                    lowInfluence,
                    new HashSet<Presence>() { lowInfluence, bloatedJellyFishInPolaris },
                    new HashSet<Conflict>() { election }
                ).SetName("AddActions Election")
            };
        }
    }
}
