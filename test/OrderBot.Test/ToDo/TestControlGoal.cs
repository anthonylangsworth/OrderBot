using NUnit.Framework;
using OrderBot.Core;
using OrderBot.ToDo;

namespace OrderBot.Test.ToDo
{
    internal class TestControlGoal
    {
        [Test]
        public void Instance()
        {
            Assert.That(ControlGoal.Instance.Name, Is.EqualTo("Control"));
            Assert.That(ControlGoal.Instance.Description, Is.EqualTo("Be the minor faction with the highest influence. Keep influence between 55% and 65%."));
            Assert.That(ControlGoal.LowerInfluenceThreshold, Is.EqualTo(0.55));
            Assert.That(ControlGoal.UpperInfluenceThreshold, Is.EqualTo(0.65));
        }

        [Test]
        [TestCaseSource(nameof(AddActions_Source))]
        public void AddActions(StarSystemMinorFaction starSystemMinorFaction,
            IReadOnlySet<StarSystemMinorFaction> systemBgsData,
            IReadOnlySet<Conflict> systemConflicts,
            IEnumerable<InfluenceSuggestion> expectedPro,
            IEnumerable<InfluenceSuggestion> expectedAnti,
            IEnumerable<SecuritySuggestion> expectedProSecurity,
            IEnumerable<ConflictSuggestion> expectedWars,
            IEnumerable<ConflictSuggestion> expectedElections)
        {
            ToDoList toDo = new(starSystemMinorFaction.MinorFaction.Name);
            ControlGoal.Instance.AddSuggestions(starSystemMinorFaction, systemBgsData, systemConflicts, toDo);
            Assert.That(toDo.Pro, Is.EquivalentTo(expectedPro).Using(DbInfluenceInitiatedSuggestionEqualityComparer.Instance));
            Assert.That(toDo.Anti, Is.EquivalentTo(expectedAnti).Using(DbInfluenceInitiatedSuggestionEqualityComparer.Instance));
            Assert.That(toDo.ProSecurity, Is.EquivalentTo(expectedProSecurity).Using(DbSecurityInitiatedSuggestionEqualityComparer.Instance));
            Assert.That(toDo.Wars, Is.EquivalentTo(expectedWars).Using(DbSecurityInitiatedSuggestionEqualityComparer.Instance));
            Assert.That(toDo.Elections, Is.EquivalentTo(expectedElections).Using(DbSecurityInitiatedSuggestionEqualityComparer.Instance));
        }

        public static IEnumerable<TestCaseData> AddActions_Source()
        {
            StarSystem polaris = new() { Name = "Polaris", LastUpdated = DateTime.UtcNow };
            MinorFaction flyingFish = new() { Name = "Flying Fish" };
            MinorFaction bloatedJellyFish = new() { Name = "Bloated Jelly Fish" };
            StarSystemMinorFaction belowLower = new()
            {
                StarSystem = polaris,
                MinorFaction = flyingFish,
                Influence = ControlGoal.LowerInfluenceThreshold - 0.01,
                SecurityLevel = null
            };
            StarSystemMinorFaction lower = new()
            {
                StarSystem = polaris,
                MinorFaction = flyingFish,
                Influence = ControlGoal.LowerInfluenceThreshold,
                SecurityLevel = null
            };
            StarSystemMinorFaction aboveLower = new()
            {
                StarSystem = polaris,
                MinorFaction = flyingFish,
                Influence = ControlGoal.LowerInfluenceThreshold + 0.01,
                SecurityLevel = null
            };
            StarSystemMinorFaction belowUpper = new()
            {
                StarSystem = polaris,
                MinorFaction = flyingFish,
                Influence = ControlGoal.UpperInfluenceThreshold - 0.01,
                SecurityLevel = SecurityLevel.High
            };
            StarSystemMinorFaction upper = new()
            {
                StarSystem = polaris,
                MinorFaction = flyingFish,
                Influence = ControlGoal.UpperInfluenceThreshold,
                SecurityLevel = SecurityLevel.Medium
            };
            StarSystemMinorFaction aboveUpper = new()
            {
                StarSystem = polaris,
                MinorFaction = flyingFish,
                Influence = ControlGoal.UpperInfluenceThreshold + 0.01,
                SecurityLevel = SecurityLevel.Low
            };
            StarSystemMinorFaction bloatedJellyFishInPolaris = new()
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
                WarType = "war",
                Status = "active"
            };

            return new[]
            {
                new TestCaseData(
                    belowLower,
                    new HashSet<StarSystemMinorFaction>() { belowLower },
                    new HashSet<Conflict>(),
                    new [] { new InfluenceSuggestion() { StarSystem = polaris, Influence = belowLower.Influence } },
                    Array.Empty<InfluenceSuggestion>(),
                    Array.Empty<SecuritySuggestion>(),
                    Array.Empty<ConflictSuggestion>(),
                    Array.Empty<ConflictSuggestion>()
                ).SetName("AddActions Below Lower"),
                new TestCaseData(
                    lower,
                    new HashSet<StarSystemMinorFaction> { lower },
                    new HashSet<Conflict>(),
                    Array.Empty<InfluenceSuggestion>(),
                    Array.Empty<InfluenceSuggestion>(),
                    Array.Empty<SecuritySuggestion>(),
                    Array.Empty<ConflictSuggestion>(),
                    Array.Empty<ConflictSuggestion>()
                ).SetName("AddActions Lower"),
                new TestCaseData(
                    aboveLower,
                    new HashSet<StarSystemMinorFaction> { aboveLower },
                    new HashSet<Conflict>(),
                    Array.Empty<InfluenceSuggestion>(),
                    Array.Empty<InfluenceSuggestion>(),
                    Array.Empty<SecuritySuggestion>(),
                    Array.Empty<ConflictSuggestion>(),
                    Array.Empty<ConflictSuggestion>()
                ).SetName("AddActions Above lower"),
                new TestCaseData(
                    belowUpper,
                    new HashSet<StarSystemMinorFaction> { belowUpper },
                    new HashSet<Conflict>(),
                    Array.Empty<InfluenceSuggestion>(),
                    Array.Empty<InfluenceSuggestion>(),
                    Array.Empty<SecuritySuggestion>(),
                    Array.Empty<ConflictSuggestion>(),
                    Array.Empty<ConflictSuggestion>()
                ).SetName("AddActions Below Upper"),
                new TestCaseData(
                    upper,
                    new HashSet<StarSystemMinorFaction> { upper },
                    new HashSet<Conflict>(),
                    Array.Empty<InfluenceSuggestion>(),
                    Array.Empty<InfluenceSuggestion>(),
                    Array.Empty<SecuritySuggestion>(),
                    Array.Empty<ConflictSuggestion>(),
                    Array.Empty<ConflictSuggestion>()
                ).SetName("AddActions Upper"),
                new TestCaseData(
                    aboveUpper,
                    new HashSet<StarSystemMinorFaction> { aboveUpper },
                    new HashSet<Conflict>(),
                    Array.Empty<InfluenceSuggestion>(),
                    new [] { new InfluenceSuggestion() { StarSystem = polaris, Influence = aboveUpper.Influence } },
                    new [] { new SecuritySuggestion() { StarSystem = polaris, SecurityLevel = aboveUpper.SecurityLevel } },
                    Array.Empty<ConflictSuggestion>(),
                    Array.Empty<ConflictSuggestion>()
                ).SetName("AddActions Above Upper"),
                new TestCaseData(
                    belowLower,
                    new HashSet<StarSystemMinorFaction>() { belowLower, bloatedJellyFishInPolaris },
                    new HashSet<Conflict>() { war },
                    Array.Empty<InfluenceSuggestion>(),
                    Array.Empty<InfluenceSuggestion>(),
                    Array.Empty<SecuritySuggestion>(),
                    new List<ConflictSuggestion>() {
                        new ConflictSuggestion()
                        {
                            StarSystem = polaris,
                            MinorFaction1 = flyingFish,
                            MinorFaction1WonDays = 2,
                            MinorFaction2 = bloatedJellyFish,
                            MinorFaction2WonDays = 1,
                            FightFor = flyingFish,
                            State = "active"
                        }
                    },
                    Array.Empty<ConflictSuggestion>()
                ).SetName("AddActions War"),
            };
        }
    }
}
