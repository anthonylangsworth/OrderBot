using NUnit.Framework;
using OrderBot.Core;
using OrderBot.ToDo;

namespace OrderBot.Test.ToDo;

internal class RetreatGoalTests
{
    [Test]
    public void Instance()
    {
        Assert.That(RetreatGoal.Instance.Name, Is.EqualTo("Retreat"));
        Assert.That(RetreatGoal.Instance.Description, Is.EqualTo("Reduce influence below 5% and keep it there."));
        Assert.That(RetreatGoal.InfluenceThreshold, Is.EqualTo(0.05));
    }

    [Test]
    [TestCaseSource(nameof(AddActions_Source))]
    public IEnumerable<Suggestion> AddActions(Presence starSystemMinorFaction, IReadOnlySet<Presence> systemPresences,
        IReadOnlySet<Conflict> systemConflicts)
    {
        return RetreatGoal.Instance.GetSuggestions(starSystemMinorFaction, systemPresences, systemConflicts);
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
                new HashSet<Conflict>()
            ).Returns(Array.Empty<Suggestion>())
             .SetName("AddActions Below"),
            new TestCaseData(
                at,
                new HashSet<Presence> { at },
                new HashSet<Conflict>()
            ).Returns(new Suggestion[] { new InfluenceSuggestion(polaris, at.MinorFaction, false, at.Influence, SuggestionDescriptions.Retreating) })
             .SetName("AddActions At"),
            new TestCaseData(
                above,
                new HashSet<Presence> { above },
                new HashSet<Conflict>()
            ).Returns(new Suggestion[] { new InfluenceSuggestion(polaris, above.MinorFaction, false, above.Influence, SuggestionDescriptions.Retreating) })
             .SetName("AddActions Above"),
            new TestCaseData(
                below,
                new HashSet<Presence>() { below, bloatedJellyFishInPolaris },
                new HashSet<Conflict>() { war }
            ).Returns(new Suggestion[]
                {
                    new ConflictSuggestion(polaris, bloatedJellyFish, war.MinorFaction2WonDays,
                        flyingFish, war.MinorFaction1WonDays, ConflictState.CloseDefeat, war.WarType)
                })
             .SetName("AddActions War"),
            new TestCaseData(
                below,
                new HashSet<Presence>() { below, bloatedJellyFishInPolaris },
                new HashSet<Conflict>() { civilWar }
            ).Returns(new Suggestion[]
                {
                    new ConflictSuggestion(polaris, bloatedJellyFish, civilWar.MinorFaction1WonDays,
                        flyingFish, civilWar.MinorFaction2WonDays, ConflictState.TotalDefeat, civilWar.WarType)
                })
             .SetName("AddActions CivilWar"),
            new TestCaseData(
                below,
                new HashSet<Presence>() { below, bloatedJellyFishInPolaris },
                new HashSet<Conflict>() { election }
            ).Returns(new Suggestion[]
                {
                    new ConflictSuggestion(polaris, bloatedJellyFish, election.MinorFaction1WonDays,
                    flyingFish, election.MinorFaction2WonDays, ConflictState.Victory, election.WarType)
                })
             .SetName("AddActions Election"),
        };
    }
}
