using NUnit.Framework;
using OrderBot.Core;
using OrderBot.ToDo;

namespace OrderBot.Test.ToDo;

internal class ExpandGoalTests
{
    [Test]
    public void Instance()
    {
        Assert.That(ExpandGoal.Instance.Name, Is.EqualTo("Expand"));
        Assert.That(ExpandGoal.Instance.Description, Is.EqualTo("Increase influence over 75% and keep it there."));
        Assert.That(ExpandGoal.InfluenceThreshold, Is.EqualTo(0.75));
    }

    [Test]
    [TestCaseSource(nameof(AddActions_Source))]
    public IEnumerable<Suggestion> AddActions(Presence starSystemMinorFaction, IReadOnlySet<Presence> systemPresences,
        IReadOnlySet<Conflict> systemConflicts)
    {
        return ExpandGoal.Instance.GetSuggestions(starSystemMinorFaction, systemPresences, systemConflicts);
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
            Influence = ExpandGoal.InfluenceThreshold - 0.01,
            SecurityLevel = null
        };
        Presence at = new()
        {
            StarSystem = polaris,
            MinorFaction = flyingFish,
            Influence = ExpandGoal.InfluenceThreshold,
            SecurityLevel = null
        };
        Presence above = new()
        {
            StarSystem = polaris,
            MinorFaction = flyingFish,
            Influence = ExpandGoal.InfluenceThreshold + 0.01,
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
            MinorFaction2WonDays = 1,
            WarType = WarType.Election,
            Status = ConflictStatus.Active
        };

        return new[]
        {
            new TestCaseData(
                below,
                new HashSet<Presence>() { below },
                new HashSet<Conflict>()
            ).Returns(new [] { new InfluenceSuggestion(polaris, flyingFish, true, below.Influence)})
             .SetName("AddActions Below"),
            new TestCaseData(
                at,
                new HashSet<Presence> { at },
                new HashSet<Conflict>()
            ).Returns(Array.Empty<Suggestion>())
             .SetName("AddActions At"),
            new TestCaseData(
                above,
                new HashSet<Presence> { above },
                new HashSet<Conflict>()
            ).Returns(Array.Empty<Suggestion>())
             .SetName("AddActions Above"),
            new TestCaseData(
                below,
                new HashSet<Presence>() { below, bloatedJellyFishInPolaris },
                new HashSet<Conflict>() { war }
            ).Returns(new List<Suggestion>()
                {
                    new ConflictSuggestion(polaris, flyingFish,war.MinorFaction1WonDays,
                        bloatedJellyFish,war.MinorFaction2WonDays, ConflictState.CloseVictory, war.WarType)
                })
             .SetName("AddActions War"),
            new TestCaseData(
                below,
                new HashSet<Presence>() { below, bloatedJellyFishInPolaris },
                new HashSet<Conflict>() { civilWar }
            ).Returns(new List<Suggestion>()
                {
                    new ConflictSuggestion(polaris, flyingFish, civilWar.MinorFaction2WonDays,
                        bloatedJellyFish,civilWar.MinorFaction1WonDays, ConflictState.TotalVictory, civilWar.WarType)
                })
             .SetName("AddActions CivilWar"),
            new TestCaseData(
                below,
                new HashSet<Presence>() { below, bloatedJellyFishInPolaris },
                new HashSet<Conflict>() { election }
            ).Returns(
                new List<Suggestion>()
                {
                    new ConflictSuggestion(polaris, flyingFish, election.MinorFaction2WonDays,
                        bloatedJellyFish, election.MinorFaction1WonDays, ConflictState.CloseDefeat, election.WarType)
                })
              .SetName("AddActions Election"),
        };
    }
}
