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
        public void AddActions(Presence starSystemMinorFaction,
            IReadOnlySet<Presence> systemPresences,
            IReadOnlySet<Conflict> systemConflicts,
            IEnumerable<InfluenceSuggestion> expectedPro,
            IEnumerable<InfluenceSuggestion> expectedAnti,
            IEnumerable<SecuritySuggestion> expectedProSecurity,
            IEnumerable<ConflictSuggestion> expectedWars,
            IEnumerable<ConflictSuggestion> expectedElections)
        {
            ToDoList toDo = new(starSystemMinorFaction.MinorFaction.Name);
            ControlGoal.Instance.AddSuggestions(starSystemMinorFaction, systemPresences, systemConflicts, toDo);
            Assert.That(toDo.Pro, Is.EquivalentTo(expectedPro));
            Assert.That(toDo.Anti, Is.EquivalentTo(expectedAnti));
            Assert.That(toDo.ProSecurity, Is.EquivalentTo(expectedProSecurity));
            Assert.That(toDo.Wars, Is.EquivalentTo(expectedWars));
            Assert.That(toDo.Elections, Is.EquivalentTo(expectedElections));
        }

        public static IEnumerable<TestCaseData> AddActions_Source()
        {
            StarSystem polaris = new() { Name = "Polaris", LastUpdated = DateTime.UtcNow };
            MinorFaction flyingFish = new() { Name = "Flying Fish" };
            MinorFaction bloatedJellyFish = new() { Name = "Bloated Jelly Fish" };
            Presence belowLower = new()
            {
                StarSystem = polaris,
                MinorFaction = flyingFish,
                Influence = ControlGoal.LowerInfluenceThreshold - 0.01,
                SecurityLevel = null
            };
            Presence lower = new()
            {
                StarSystem = polaris,
                MinorFaction = flyingFish,
                Influence = ControlGoal.LowerInfluenceThreshold,
                SecurityLevel = null
            };
            Presence aboveLower = new()
            {
                StarSystem = polaris,
                MinorFaction = flyingFish,
                Influence = ControlGoal.LowerInfluenceThreshold + 0.01,
                SecurityLevel = null
            };
            Presence belowUpper = new()
            {
                StarSystem = polaris,
                MinorFaction = flyingFish,
                Influence = ControlGoal.UpperInfluenceThreshold - 0.01,
                SecurityLevel = SecurityLevel.High
            };
            Presence upper = new()
            {
                StarSystem = polaris,
                MinorFaction = flyingFish,
                Influence = ControlGoal.UpperInfluenceThreshold,
                SecurityLevel = SecurityLevel.Medium
            };
            Presence aboveUpper = new()
            {
                StarSystem = polaris,
                MinorFaction = flyingFish,
                Influence = ControlGoal.UpperInfluenceThreshold + 0.01,
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
                    belowLower,
                    new HashSet<Presence>() { belowLower },
                    new HashSet<Conflict>(),
                    new [] { new InfluenceSuggestion() { StarSystem = polaris, Influence = belowLower.Influence } },
                    Array.Empty<InfluenceSuggestion>(),
                    Array.Empty<SecuritySuggestion>(),
                    Array.Empty<ConflictSuggestion>(),
                    Array.Empty<ConflictSuggestion>()
                ).SetName("AddActions Below Lower"),
                new TestCaseData(
                    lower,
                    new HashSet<Presence> { lower },
                    new HashSet<Conflict>(),
                    Array.Empty<InfluenceSuggestion>(),
                    Array.Empty<InfluenceSuggestion>(),
                    Array.Empty<SecuritySuggestion>(),
                    Array.Empty<ConflictSuggestion>(),
                    Array.Empty<ConflictSuggestion>()
                ).SetName("AddActions Lower"),
                new TestCaseData(
                    aboveLower,
                    new HashSet<Presence> { aboveLower },
                    new HashSet<Conflict>(),
                    Array.Empty<InfluenceSuggestion>(),
                    Array.Empty<InfluenceSuggestion>(),
                    Array.Empty<SecuritySuggestion>(),
                    Array.Empty<ConflictSuggestion>(),
                    Array.Empty<ConflictSuggestion>()
                ).SetName("AddActions Above lower"),
                new TestCaseData(
                    belowUpper,
                    new HashSet<Presence> { belowUpper },
                    new HashSet<Conflict>(),
                    Array.Empty<InfluenceSuggestion>(),
                    Array.Empty<InfluenceSuggestion>(),
                    Array.Empty<SecuritySuggestion>(),
                    Array.Empty<ConflictSuggestion>(),
                    Array.Empty<ConflictSuggestion>()
                ).SetName("AddActions Below Upper"),
                new TestCaseData(
                    upper,
                    new HashSet<Presence> { upper },
                    new HashSet<Conflict>(),
                    Array.Empty<InfluenceSuggestion>(),
                    Array.Empty<InfluenceSuggestion>(),
                    Array.Empty<SecuritySuggestion>(),
                    Array.Empty<ConflictSuggestion>(),
                    Array.Empty<ConflictSuggestion>()
                ).SetName("AddActions Upper"),
                new TestCaseData(
                    aboveUpper,
                    new HashSet<Presence> { aboveUpper },
                    new HashSet<Conflict>(),
                    Array.Empty<InfluenceSuggestion>(),
                    new [] { new InfluenceSuggestion() { StarSystem = polaris, Influence = aboveUpper.Influence } },
                    new [] { new SecuritySuggestion() { StarSystem = polaris, SecurityLevel = aboveUpper.SecurityLevel } },
                    Array.Empty<ConflictSuggestion>(),
                    Array.Empty<ConflictSuggestion>()
                ).SetName("AddActions Above Upper"),
                new TestCaseData(
                    belowLower,
                    new HashSet<Presence>() { belowLower, bloatedJellyFishInPolaris },
                    new HashSet<Conflict>() { war },
                    Array.Empty<InfluenceSuggestion>(),
                    Array.Empty<InfluenceSuggestion>(),
                    Array.Empty<SecuritySuggestion>(),
                    new List<ConflictSuggestion>() {
                        new ConflictSuggestion()
                        {
                            StarSystem = polaris,
                            FightFor = flyingFish,
                            FightForWonDays = war.MinorFaction1WonDays,
                            FightAgainst = bloatedJellyFish,
                            FightAgainstWonDays = war.MinorFaction2WonDays,
                            State = ConflictState.CloseVictory,
                            WarType = war.WarType
                        }
                    },
                    Array.Empty<ConflictSuggestion>()
                ).SetName("AddActions War"),
                new TestCaseData(
                    belowLower,
                    new HashSet<Presence>() { belowLower, bloatedJellyFishInPolaris },
                    new HashSet<Conflict>() { civilWar },
                    Array.Empty<InfluenceSuggestion>(),
                    Array.Empty<InfluenceSuggestion>(),
                    Array.Empty<SecuritySuggestion>(),
                    new List<ConflictSuggestion>() {
                        new ConflictSuggestion()
                        {
                            StarSystem = polaris,
                            FightFor = flyingFish,
                            FightForWonDays = civilWar.MinorFaction2WonDays,
                            FightAgainst = bloatedJellyFish,
                            FightAgainstWonDays = civilWar.MinorFaction1WonDays,
                            State = ConflictState.TotalVictory,
                            WarType = civilWar.WarType
                        }
                    },
                    Array.Empty<ConflictSuggestion>()
                ).SetName("AddActions CivilWar"),
                new TestCaseData(
                    belowLower,
                    new HashSet<Presence>() { belowLower, bloatedJellyFishInPolaris },
                    new HashSet<Conflict>() { election },
                    Array.Empty<InfluenceSuggestion>(),
                    Array.Empty<InfluenceSuggestion>(),
                    Array.Empty<SecuritySuggestion>(),
                    Array.Empty<ConflictSuggestion>(),
                    new List<ConflictSuggestion>() {
                        new ConflictSuggestion()
                        {
                            StarSystem = polaris,
                            FightFor = flyingFish,
                            FightForWonDays = election.MinorFaction2WonDays,
                            FightAgainst = bloatedJellyFish,
                            FightAgainstWonDays = election.MinorFaction1WonDays,
                            State = ConflictState.CloseDefeat,
                            WarType = election.WarType
                        }
                    }
                ).SetName("AddActions Election"),
            };
        }
    }
}
