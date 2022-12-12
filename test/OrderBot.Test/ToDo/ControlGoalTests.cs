using NUnit.Framework;
using OrderBot.Core;
using OrderBot.ToDo;

namespace OrderBot.Test.ToDo;

internal class ControlGoalTests
{
    [Test]
    public void Instance()
    {
        Assert.That(ControlGoal.Instance.Name, Is.EqualTo("Control"));
        Assert.That(ControlGoal.Instance.Description, Is.EqualTo("Have the highest influence. Keep influence between 55% and 65%."));
        Assert.That(ControlGoal.LowerInfluenceThreshold, Is.EqualTo(0.55));
        Assert.That(ControlGoal.UpperInfluenceThreshold, Is.EqualTo(0.65));
    }

    [Test]
    [TestCaseSource(nameof(AddActions_Source))]
    public IEnumerable<Suggestion> AddActions(Presence starSystemMinorFaction, IReadOnlySet<Presence> systemPresences,
        IReadOnlySet<Conflict> systemConflicts)
    {
        return ControlGoal.Instance.GetSuggestions(starSystemMinorFaction, systemPresences, systemConflicts);
    }

    public static IEnumerable<TestCaseData> AddActions_Source()
    {
        StarSystem polaris = new() { Name = "Polaris", LastUpdated = DateTime.UtcNow };
        MinorFaction flyingFish = new() { Name = "Flying Fish" };
        MinorFaction bloatedJellyFish = new() { Name = "Bloated Jelly Fish" };
        MinorFaction blueMarlins = new() { Name = "Marlins" };
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
        Presence marlinsRetreating = new()
        {
            StarSystem = polaris,
            MinorFaction = blueMarlins,
            Influence = RetreatGoal.InfluenceThreshold - 0.01,
            States = new List<State> { new State { Name = State.Retreat } },
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
                new HashSet<Conflict>()
            ).Returns(new [] { new InfluenceSuggestion(polaris, belowLower.MinorFaction, true, belowLower.Influence) })
             .SetName("AddActions Below Lower"),
            new TestCaseData(
                lower,
                new HashSet<Presence> { lower },
                new HashSet<Conflict>()
            ).Returns(Array.Empty<Suggestion>())
             .SetName("AddActions Lower"),
            new TestCaseData(
                aboveLower,
                new HashSet<Presence> { aboveLower },
                new HashSet<Conflict>()
            ).Returns(Array.Empty<Suggestion>())
             .SetName("AddActions Above lower"),
            new TestCaseData(
                belowUpper,
                new HashSet<Presence> { belowUpper },
                new HashSet<Conflict>()
            ).Returns(Array.Empty<Suggestion>())
             .SetName("AddActions Below Upper"),
            new TestCaseData(
                upper,
                new HashSet<Presence> { upper },
                new HashSet<Conflict>()
            ).Returns(Array.Empty<Suggestion>())
             .SetName("AddActions Upper"),
            new TestCaseData(
                aboveUpper,
                new HashSet<Presence> { aboveUpper },
                new HashSet<Conflict>()
            ).Returns(new Suggestion[] {
                    new InfluenceSuggestion(polaris, flyingFish, false, aboveUpper.Influence),
                    new SecuritySuggestion(polaris, aboveUpper.SecurityLevel)
                 })
             .SetName("AddActions Above Upper"),
            new TestCaseData(
                belowLower,
                new HashSet<Presence>() { belowLower, bloatedJellyFishInPolaris },
                new HashSet<Conflict>() { war }
            ).Returns(new Suggestion[]
                {
                    new ConflictSuggestion(polaris, flyingFish, war.MinorFaction1WonDays,
                        bloatedJellyFish, war.MinorFaction2WonDays, ConflictState.CloseVictory, war.WarType)
                })
             .SetName("AddActions War"),
            new TestCaseData(
                belowLower,
                new HashSet<Presence>() { belowLower, bloatedJellyFishInPolaris },
                new HashSet<Conflict>() { civilWar }
            ).Returns(new Suggestion[]
                {
                    new ConflictSuggestion(polaris, flyingFish,civilWar.MinorFaction2WonDays,
                        bloatedJellyFish, civilWar.MinorFaction1WonDays, ConflictState.TotalVictory, civilWar.WarType)
                })
             .SetName("AddActions CivilWar"),
            new TestCaseData(
                belowUpper,
                new HashSet<Presence>() { belowUpper, marlinsRetreating },
                new HashSet<Conflict>()
            ).Returns(new Suggestion[]
                {
                    new InfluenceSuggestion(marlinsRetreating.StarSystem, marlinsRetreating.MinorFaction, true,
                        marlinsRetreating.Influence, SuggestionDescriptions.AvoidRetreat),
                })
             .SetName("AddActions with Retreat"),
        };
    }
}
