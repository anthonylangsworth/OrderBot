using NUnit.Framework;
using OrderBot.Core;
using OrderBot.ToDo;

namespace OrderBot.Test.ToDo
{
    internal class TestMaintainGoal
    {
        [Test]
        public void Instance()
        {
            Assert.That(MaintainGoal.Instance.Name, Is.EqualTo("Maintain"));
            Assert.That(MaintainGoal.Instance.Description, Is.EqualTo("Maintain a presence in the system but do not control it."));
            Assert.That(MaintainGoal.LowerInfluenceThreshold, Is.EqualTo(0.1));
            Assert.That(MaintainGoal.MaxInfuenceGap, Is.EqualTo(0.03));
        }
        [Test]
        [TestCaseSource(nameof(AddActions_Source))]
        public void AddActions(Presence presence,
            IReadOnlySet<Presence> systemPresences,
            IReadOnlySet<Conflict> systemConflicts,
            IEnumerable<Suggestion> expectedSuggestions)
        {
            ToDoList toDo = new(presence.MinorFaction.Name);
            MaintainGoal.Instance.AddSuggestions(presence, systemPresences, systemConflicts, toDo);
            Assert.That(toDo.Suggestions, Is.EquivalentTo(expectedSuggestions));
        }

        public static IEnumerable<TestCaseData> AddActions_Source()
        {
            StarSystem polaris = new() { Name = "Polaris", LastUpdated = DateTime.UtcNow };
            MinorFaction blackSwans = new() { Name = "Black Swans" };
            MinorFaction flyingFish = new() { Name = "Flying Fish" };
            MinorFaction bloatedJellyFish = new() { Name = "Bloated Jelly Fish" };
            Presence blackSwanInPolaris = new()
            {
                StarSystem = polaris,
                MinorFaction = blackSwans,
                Influence = 0.2,
                SecurityLevel = null
            };
            Presence flyingFishBelowLower = new()
            {
                StarSystem = polaris,
                MinorFaction = flyingFish,
                Influence = MaintainGoal.LowerInfluenceThreshold - 0.01,
                SecurityLevel = null
            };
            Presence flyingFishAtLower = new()
            {
                StarSystem = polaris,
                MinorFaction = flyingFish,
                Influence = MaintainGoal.LowerInfluenceThreshold,
                SecurityLevel = null
            };
            Presence flyingFishAboveLower = new()
            {
                StarSystem = polaris,
                MinorFaction = flyingFish,
                Influence = MaintainGoal.LowerInfluenceThreshold + 0.01,
                SecurityLevel = null
            };
            Presence flyingFishControl = new()
            {
                StarSystem = polaris,
                MinorFaction = flyingFish,
                Influence = 0.9,
                SecurityLevel = null
            };
            Presence bloatedJellyFishInPolaris = new()
            {
                StarSystem = polaris,
                MinorFaction = bloatedJellyFish,
                Influence = ControlGoal.UpperInfluenceThreshold,
                SecurityLevel = null
            };
            Conflict flyingVsJellyFishWar = new()
            {
                StarSystem = polaris,
                MinorFaction1 = flyingFish,
                MinorFaction1WonDays = 2,
                MinorFaction2 = bloatedJellyFish,
                MinorFaction2WonDays = 1,
                WarType = WarType.War,
                Status = ConflictStatus.Active
            };
            Conflict swanVsFlyingFishElection = new()
            {
                StarSystem = polaris,
                MinorFaction1 = flyingFish,
                MinorFaction1WonDays = 3,
                MinorFaction2 = blackSwans,
                MinorFaction2WonDays = 2,
                WarType = WarType.Election,
                Status = ConflictStatus.Active
            };


            return new[]
            {
                new TestCaseData(
                    flyingFishBelowLower,
                    new HashSet<Presence>() { flyingFishBelowLower },
                    new HashSet<Conflict>(),
                    Array.Empty<Suggestion>()
                ).SetName("AddActions BelowLower Single Presence"),
                new TestCaseData(
                    flyingFishAtLower,
                    new HashSet<Presence> { flyingFishAtLower },
                    new HashSet<Conflict>(),
                    Array.Empty<Suggestion>()
                ).SetName("AddActions AtLower Single Presence"),
                new TestCaseData(
                    flyingFishAboveLower,
                    new HashSet<Presence> { flyingFishAboveLower },
                    new HashSet<Conflict>(),
                    Array.Empty<Suggestion>()
                ).SetName("AddActions AboveLower Single Presence"),
                new TestCaseData(
                    flyingFishBelowLower,
                    new HashSet<Presence>() { flyingFishBelowLower, blackSwanInPolaris },
                    new HashSet<Conflict>(),
                    new Suggestion[] { new InfluenceSuggestion() { StarSystem = polaris, Influence = flyingFishBelowLower.Influence, Pro = true } }
                ).SetName("AddActions BelowLower"),
                new TestCaseData(
                    flyingFishAtLower,
                    new HashSet<Presence> { flyingFishAtLower, blackSwanInPolaris },
                    new HashSet<Conflict>(),
                    Array.Empty<Suggestion>()
                ).SetName("AddActions AtLower"),
                new TestCaseData(
                    flyingFishAboveLower,
                    new HashSet<Presence> { flyingFishAboveLower, blackSwanInPolaris },
                    new HashSet<Conflict>(),
                    Array.Empty<Suggestion>()
                ).SetName("AddActions AboveLower"),
                new TestCaseData(
                    flyingFishControl,
                    new HashSet<Presence> { flyingFishControl, blackSwanInPolaris },
                    new HashSet<Conflict>(),
                    new Suggestion[] { new InfluenceSuggestion() { StarSystem = polaris, Influence = flyingFishControl.Influence, Pro = false, Description = "Avoid Control" } }
                ).SetName("AddActions Controlling"),
                new TestCaseData(
                    flyingFishBelowLower,
                    new HashSet<Presence>() { flyingFishBelowLower, bloatedJellyFishInPolaris },
                    new HashSet<Conflict>() { flyingVsJellyFishWar },
                    new Suggestion[]
                    {
                        new ConflictSuggestion()
                        {
                            StarSystem = polaris,
                            FightFor = bloatedJellyFish,
                            FightForWonDays = flyingVsJellyFishWar.MinorFaction2WonDays,
                            FightAgainst = flyingFish,
                            FightAgainstWonDays = flyingVsJellyFishWar.MinorFaction1WonDays,
                            State = ConflictState.CloseDefeat,
                            WarType = flyingVsJellyFishWar.WarType,
                            Description = "Avoid Control"
                        }
                    }
                ).SetName("AddActions War Against Controlling Faction"),
                new TestCaseData(
                    flyingFishBelowLower,
                    new HashSet<Presence>() { flyingFishBelowLower, blackSwanInPolaris },
                    new HashSet<Conflict>() { swanVsFlyingFishElection },
                    new Suggestion[]
                    {
                        new ConflictSuggestion()
                        {
                            StarSystem = polaris,
                            FightFor = blackSwans,
                            FightForWonDays = swanVsFlyingFishElection.MinorFaction2WonDays,
                            FightAgainst = flyingFish,
                            FightAgainstWonDays = swanVsFlyingFishElection.MinorFaction1WonDays,
                            State = ConflictState.CloseDefeat,
                            WarType = swanVsFlyingFishElection.WarType,
                            Description = "Avoid Control"
                        }
                    }
                ).SetName("AddActions Election Against Faction when Controlling"),
                new TestCaseData(
                    flyingFishBelowLower,
                    new HashSet<Presence>() { flyingFishBelowLower, blackSwanInPolaris, bloatedJellyFishInPolaris },
                    new HashSet<Conflict>() { swanVsFlyingFishElection },
                    new Suggestion[]
                    {
                        new ConflictSuggestion()
                        {
                            StarSystem = polaris,
                            FightFor = flyingFish,
                            FightForWonDays = swanVsFlyingFishElection.MinorFaction1WonDays,
                            FightAgainst = blackSwans,
                            FightAgainstWonDays = swanVsFlyingFishElection.MinorFaction2WonDays,
                            State = ConflictState.CloseVictory,
                            WarType = swanVsFlyingFishElection.WarType
                        }
                    }
                ).SetName("AddActions Election Against Faction when Not Controlling")
            };
        }
    }
}