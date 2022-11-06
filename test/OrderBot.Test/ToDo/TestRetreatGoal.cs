using NUnit.Framework;
using OrderBot.Core;
using OrderBot.ToDo;

namespace OrderBot.Test.ToDo
{
    internal class TestRetreatGoal
    {
        [Test]
        public void Instance()
        {
            Assert.That(RetreatGoal.Instance.Name, Is.EqualTo("Retreat"));
            Assert.That(RetreatGoal.Instance.Description, Is.EqualTo("Retreat from the system by reducing influence below 5% and keeping it there."));
            Assert.That(RetreatGoal.InfluenceThreshold, Is.EqualTo(0.05));
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
            RetreatGoal.Instance.AddSuggestions(starSystemMinorFaction, systemPresences, systemConflicts, toDo);
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
            Presence below = new()
            {
                StarSystem = polaris,
                MinorFaction = flyingFish,
                Influence = RetreatGoal.InfluenceThreshold - 0.01,
                SecurityLevel = null
            };
            Presence at = new()
            {
                StarSystem = polaris,
                MinorFaction = flyingFish,
                Influence = RetreatGoal.InfluenceThreshold,
                SecurityLevel = null
            };
            Presence above = new()
            {
                StarSystem = polaris,
                MinorFaction = flyingFish,
                Influence = RetreatGoal.InfluenceThreshold + 0.01,
                SecurityLevel = null
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
                MinorFaction2WonDays = 0,
                WarType = WarType.Election,
                Status = ConflictStatus.Active
            };

            return new[]
            {
                new TestCaseData(
                    below,
                    new HashSet<Presence>() { below },
                    new HashSet<Conflict>(),
                    Array.Empty<InfluenceSuggestion>(),
                    Array.Empty<InfluenceSuggestion>(),
                    Array.Empty<SecuritySuggestion>(),
                    Array.Empty<ConflictSuggestion>(),
                    Array.Empty<ConflictSuggestion>()
                ).SetName("AddActions Below"),
                new TestCaseData(
                    at,
                    new HashSet<Presence> { at },
                    new HashSet<Conflict>(),
                    Array.Empty<InfluenceSuggestion>(),
                    new [] { new InfluenceSuggestion() { StarSystem = polaris, Influence = at.Influence } },
                    Array.Empty<SecuritySuggestion>(),
                    Array.Empty<ConflictSuggestion>(),
                    Array.Empty<ConflictSuggestion>()
                ).SetName("AddActions At"),
                new TestCaseData(
                    above,
                    new HashSet<Presence> { above },
                    new HashSet<Conflict>(),
                    Array.Empty<InfluenceSuggestion>(),
                    new [] { new InfluenceSuggestion() { StarSystem = polaris, Influence = above.Influence } },
                    Array.Empty<SecuritySuggestion>(),
                    Array.Empty<ConflictSuggestion>(),
                    Array.Empty<ConflictSuggestion>()
                ).SetName("AddActions Above"),
                new TestCaseData(
                    below,
                    new HashSet<Presence>() { below, bloatedJellyFishInPolaris },
                    new HashSet<Conflict>() { war },
                    Array.Empty<InfluenceSuggestion>(),
                    Array.Empty<InfluenceSuggestion>(),
                    Array.Empty<SecuritySuggestion>(),
                    new List<ConflictSuggestion>() {
                        new ConflictSuggestion()
                        {
                            StarSystem = polaris,
                            FightFor = bloatedJellyFish,
                            FightForWonDays = war.MinorFaction2WonDays,
                            FightAgainst = flyingFish,
                            FightAgainstWonDays = war.MinorFaction1WonDays,
                            State = ConflictState.CloseDefeat,
                            WarType = war.WarType
                        }
                    },
                    Array.Empty<ConflictSuggestion>()
                ).SetName("AddActions War"),
                new TestCaseData(
                    below,
                    new HashSet<Presence>() { below, bloatedJellyFishInPolaris },
                    new HashSet<Conflict>() { civilWar },
                    Array.Empty<InfluenceSuggestion>(),
                    Array.Empty<InfluenceSuggestion>(),
                    Array.Empty<SecuritySuggestion>(),
                    new List<ConflictSuggestion>() {
                        new ConflictSuggestion()
                        {
                            StarSystem = polaris,
                            FightFor = bloatedJellyFish,
                            FightForWonDays = civilWar.MinorFaction1WonDays,
                            FightAgainst = flyingFish,
                            FightAgainstWonDays = civilWar.MinorFaction2WonDays,
                            State = ConflictState.TotalDefeat,
                            WarType = civilWar.WarType
                        }
                    },
                    Array.Empty<ConflictSuggestion>()
                ).SetName("AddActions CivilWar"),
                new TestCaseData(
                    below,
                    new HashSet<Presence>() { below, bloatedJellyFishInPolaris },
                    new HashSet<Conflict>() { election },
                    Array.Empty<InfluenceSuggestion>(),
                    Array.Empty<InfluenceSuggestion>(),
                    Array.Empty<SecuritySuggestion>(),
                    Array.Empty<ConflictSuggestion>(),
                    new List<ConflictSuggestion>() {
                        new ConflictSuggestion()
                        {
                            StarSystem = polaris,
                            FightFor = bloatedJellyFish,
                            FightForWonDays = election.MinorFaction1WonDays,
                            FightAgainst = flyingFish,
                            FightAgainstWonDays = election.MinorFaction2WonDays,
                            State = ConflictState.Victory,
                            WarType = election.WarType
                        }
                    }
                ).SetName("AddActions Election"),
            };
        }
    }
}
