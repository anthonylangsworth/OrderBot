using NUnit.Framework;
using OrderBot.Core;
using OrderBot.ToDo;

namespace OrderBot.Test.ToDo;

internal class MaintainGoalTests
{
    [Test]
    public void Instance()
    {
        Assert.That(MaintainGoal.Instance.Name, Is.EqualTo("Maintain"));
        Assert.That(MaintainGoal.Instance.Description, Is.EqualTo("Keep influence above 8% and below the controlling minor faction."));
        Assert.That(MaintainGoal.LowerInfluenceThreshold, Is.EqualTo(0.08));
        Assert.That(MaintainGoal.MaxInfuenceGap, Is.EqualTo(0.03));
    }
    [Test]
    [TestCaseSource(nameof(AddActions_Source))]
    public IEnumerable<Suggestion> AddActions(Presence presence, IReadOnlySet<Presence> systemPresences,
        IReadOnlySet<Conflict> systemConflicts)
    {
        return MaintainGoal.Instance.GetSuggestions(presence, systemPresences, systemConflicts);
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
                new HashSet<Conflict>()
            ).Returns(Array.Empty<Suggestion>())
             .SetName("AddActions BelowLower Single Presence"),
            new TestCaseData(
                flyingFishAtLower,
                new HashSet<Presence> { flyingFishAtLower },
                new HashSet<Conflict>()
            ).Returns(Array.Empty<Suggestion>())
             .SetName("AddActions AtLower Single Presence"),
            new TestCaseData(
                flyingFishAboveLower,
                new HashSet<Presence> { flyingFishAboveLower },
                new HashSet<Conflict>()
            ).Returns(Array.Empty<Suggestion>())
             .SetName("AddActions AboveLower Single Presence"),
            new TestCaseData(
                flyingFishBelowLower,
                new HashSet<Presence>() { flyingFishBelowLower, blackSwanInPolaris },
                new HashSet<Conflict>()
            ).Returns(new Suggestion[] { new InfluenceSuggestion(polaris, flyingFishBelowLower.MinorFaction, true, flyingFishBelowLower.Influence) })
             .SetName("AddActions BelowLower"),
            new TestCaseData(
                flyingFishAtLower,
                new HashSet<Presence> { flyingFishAtLower, blackSwanInPolaris },
                new HashSet<Conflict>()
            ).Returns(Array.Empty<Suggestion>())
             .SetName("AddActions AtLower"),
            new TestCaseData(
                flyingFishAboveLower,
                new HashSet<Presence> { flyingFishAboveLower, blackSwanInPolaris },
                new HashSet<Conflict>()
            ).Returns(Array.Empty<Suggestion>())
             .SetName("AddActions AboveLower"),
            new TestCaseData(
                flyingFishControl,
                new HashSet<Presence> { flyingFishControl, blackSwanInPolaris },
                new HashSet<Conflict>()
            ).Returns(new Suggestion[] { new InfluenceSuggestion(polaris, flyingFishControl.MinorFaction, false, flyingFishControl.Influence, SuggestionDescriptions.AvoidControl ) })
             .SetName("AddActions Controlling"),
            new TestCaseData(
                flyingFishBelowLower,
                new HashSet<Presence>() { flyingFishBelowLower, bloatedJellyFishInPolaris },
                new HashSet<Conflict>() { flyingVsJellyFishWar }
            ).Returns(new Suggestion[]
                {
                    new ConflictSuggestion(
                        polaris, bloatedJellyFish, flyingVsJellyFishWar.MinorFaction2WonDays,
                        flyingFish, flyingVsJellyFishWar.MinorFaction1WonDays, ConflictState.CloseDefeat, flyingVsJellyFishWar.WarType, SuggestionDescriptions.AvoidControl)
                })
             .SetName("AddActions War Against Controlling Faction"),
            new TestCaseData(
                flyingFishBelowLower,
                new HashSet<Presence>() { flyingFishBelowLower, blackSwanInPolaris },
                new HashSet<Conflict>() { swanVsFlyingFishElection }
            ).Returns(new Suggestion[]
                {
                    new ConflictSuggestion(
                        polaris, blackSwans, swanVsFlyingFishElection.MinorFaction2WonDays,
                        flyingFish, swanVsFlyingFishElection.MinorFaction1WonDays, ConflictState.CloseDefeat, swanVsFlyingFishElection.WarType, SuggestionDescriptions.AvoidControl)
                })
             .SetName("AddActions Election Against Faction when Controlling"),
            new TestCaseData(
                flyingFishBelowLower,
                new HashSet<Presence>() { flyingFishBelowLower, blackSwanInPolaris, bloatedJellyFishInPolaris },
                new HashSet<Conflict>() { swanVsFlyingFishElection }
            ).Returns(new Suggestion[]
                {
                    new ConflictSuggestion(
                        polaris, flyingFish, swanVsFlyingFishElection.MinorFaction1WonDays,
                        blackSwans, swanVsFlyingFishElection.MinorFaction2WonDays, ConflictState.CloseVictory, swanVsFlyingFishElection.WarType)
                })
             .SetName("AddActions Election Against Faction when Not Controlling")
        };
    }
}
