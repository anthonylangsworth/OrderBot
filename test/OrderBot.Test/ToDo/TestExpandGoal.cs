﻿using NUnit.Framework;
using OrderBot.Core;
using OrderBot.ToDo;

namespace OrderBot.Test.ToDo
{
    internal class TestExpandGoal
    {
        [Test]
        public void Instance()
        {
            Assert.That(ExpandGoal.Instance.Name, Is.EqualTo("Expand"));
            Assert.That(ExpandGoal.Instance.Description, Is.EqualTo("Expand this minor faction out of this system."));
            Assert.That(ExpandGoal.InfluenceThreshold, Is.EqualTo(0.75));
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
            ExpandGoal.Instance.AddSuggestions(starSystemMinorFaction, systemBgsData, systemConflicts, toDo);
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
            StarSystemMinorFaction below = new()
            {
                StarSystem = polaris,
                MinorFaction = flyingFish,
                Influence = ExpandGoal.InfluenceThreshold - 0.01,
                SecurityLevel = null
            };
            StarSystemMinorFaction at = new()
            {
                StarSystem = polaris,
                MinorFaction = flyingFish,
                Influence = ExpandGoal.InfluenceThreshold,
                SecurityLevel = null
            };
            StarSystemMinorFaction above = new()
            {
                StarSystem = polaris,
                MinorFaction = flyingFish,
                Influence = ExpandGoal.InfluenceThreshold + 0.01,
                SecurityLevel = null
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
                    below,
                    new HashSet<StarSystemMinorFaction>() { below },
                    new HashSet<Conflict>(),
                    new [] { new InfluenceSuggestion() { StarSystem = polaris, Influence = below.Influence } },
                    Array.Empty<InfluenceSuggestion>(),
                    Array.Empty<SecuritySuggestion>(),
                    Array.Empty<ConflictSuggestion>(),
                    Array.Empty<ConflictSuggestion>()
                ).SetName("AddActions Below"),
                new TestCaseData(
                    at,
                    new HashSet<StarSystemMinorFaction> { at },
                    new HashSet<Conflict>(),
                    Array.Empty<InfluenceSuggestion>(),
                    Array.Empty<InfluenceSuggestion>(),
                    Array.Empty<SecuritySuggestion>(),
                    Array.Empty<ConflictSuggestion>(),
                    Array.Empty<ConflictSuggestion>()
                ).SetName("AddActions At"),
                new TestCaseData(
                    above,
                    new HashSet<StarSystemMinorFaction> { above },
                    new HashSet<Conflict>(),
                    Array.Empty<InfluenceSuggestion>(),
                    Array.Empty<InfluenceSuggestion>(),
                    Array.Empty<SecuritySuggestion>(),
                    Array.Empty<ConflictSuggestion>(),
                    Array.Empty<ConflictSuggestion>()
                ).SetName("AddActions Above"),
                new TestCaseData(
                    below,
                    new HashSet<StarSystemMinorFaction>() { below, bloatedJellyFishInPolaris },
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
                            State = ConflictState.CloseVictory
                        }
                    },
                    Array.Empty<ConflictSuggestion>()
                ).SetName("AddActions War"),
                new TestCaseData(
                    below,
                    new HashSet<StarSystemMinorFaction>() { below, bloatedJellyFishInPolaris },
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
                            State = ConflictState.TotalVictory
                        }
                    },
                    Array.Empty<ConflictSuggestion>()
                ).SetName("AddActions CivilWar"),
                new TestCaseData(
                    below,
                    new HashSet<StarSystemMinorFaction>() { below, bloatedJellyFishInPolaris },
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
                            State = ConflictState.CloseDefeat
                        }
                    }
                ).SetName("AddActions Election"),
            };
        }
    }
}