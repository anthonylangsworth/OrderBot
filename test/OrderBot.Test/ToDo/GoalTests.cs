using NUnit.Framework;
using OrderBot.Core;
using OrderBot.ToDo;

namespace OrderBot.Test.ToDo;

internal class GoalTests
{
    [Test]
    public void CheckAddActionsPreconditions_MultipleSystems()
    {
        StarSystem betelgeuse = new() { Name = "Betelgeuse" };
        StarSystem sirius = new() { Name = "Sirius" };
        MinorFaction gumChewers = new() { Name = "Gum Chewers" };
        HashSet<Presence> bgsData = new()
        {
            new Presence() { StarSystem = betelgeuse, MinorFaction = gumChewers, Influence = 0 },
            new Presence() { StarSystem = sirius, MinorFaction = gumChewers, Influence = 0 }
        };
        Assert.That(
            () => Goal.CheckAddActionsPreconditions(bgsData.First(), bgsData, new HashSet<Conflict>()),
            Throws.ArgumentException.And.Property("Message").EqualTo("All systemPresences must be for star system Betelgeuse"));
    }

    [Test]
    public void CheckAddActionsPreconditions_MinorFactionNotInList()
    {
        StarSystem betelgeuse = new() { Name = "Betelgeuse" };
        MinorFaction gumChewers = new() { Name = "Gum Chewers" };
        MinorFaction funnyWalkers = new() { Name = "Funny Walkers" };
        MinorFaction bunnyHoppers = new() { Name = "Bunny Hoppoers" };
        HashSet<Presence> bgsData = new()
        {
            new Presence() { StarSystem = betelgeuse, MinorFaction = gumChewers, Influence = 0 },
            new Presence() { StarSystem = betelgeuse, MinorFaction = funnyWalkers, Influence = 0 }
        };
        Presence different = new() { StarSystem = betelgeuse, MinorFaction = bunnyHoppers, Influence = 0.5 };
        Assert.That(
            () => Goal.CheckAddActionsPreconditions(different, bgsData, new HashSet<Conflict>()),
            Throws.ArgumentException.And.Property("Message").EqualTo("systemPresences must contain starSystemMinorFaction"));
    }

    [Test]
    public void CheckAddActionsPreconditions_MultipleConflicts()
    {
        StarSystem betelgeuse = new() { Name = "Betelgeuse" };
        StarSystem sirius = new() { Name = "Sirius" };
        MinorFaction gumChewers = new() { Name = "Gum Chewers" };
        MinorFaction funnyWalkers = new() { Name = "Funny Walkers" };
        MinorFaction bunnyHoppers = new() { Name = "Bunny Hoppoers" };
        MinorFaction sliders = new() { Name = "Sliders" };
        HashSet<Presence> bgsData = new()
        {
            new Presence() { StarSystem = betelgeuse, MinorFaction = gumChewers, Influence = 0 },
            new Presence() { StarSystem = betelgeuse, MinorFaction = funnyWalkers, Influence = 0 },
            new Presence() { StarSystem = betelgeuse, MinorFaction = sliders, Influence = 0 }
        };
        HashSet<Conflict> conflicts = new()
        {
            new Conflict() { StarSystem = betelgeuse, MinorFaction1 = bunnyHoppers, MinorFaction2 = gumChewers },
            new Conflict() { StarSystem = sirius, MinorFaction1 = funnyWalkers, MinorFaction2 = sliders  }
        };
        Assert.That(
            () => Goal.CheckAddActionsPreconditions(bgsData.First(), bgsData, conflicts),
            Throws.ArgumentException.And.Property("Message").EqualTo("All systemConflicts must be in star system Betelgeuse"));
    }

    [Test]
    public void CheckAddActionsPreconditions_ConflictMinorFactionNotInSystemBgsData()
    {
        StarSystem betelgeuse = new() { Name = "Betelgeuse" };
        StarSystem sirius = new() { Name = "Sirius" };
        MinorFaction gumChewers = new() { Name = "Gum Chewers" };
        MinorFaction funnyWalkers = new() { Name = "Funny Walkers" };
        MinorFaction bunnyHoppers = new() { Name = "Bunny Hoppoers" };
        MinorFaction sliders = new() { Name = "Sliders" };
        HashSet<Presence> bgsData = new()
        {
            new Presence() { StarSystem = betelgeuse, MinorFaction = gumChewers, Influence = 0 },
        };
        HashSet<Conflict> conflicts = new()
        {
            new Conflict() { StarSystem = betelgeuse, MinorFaction1 = bunnyHoppers, MinorFaction2 = gumChewers },
            new Conflict() { StarSystem = betelgeuse, MinorFaction1 = funnyWalkers, MinorFaction2 = sliders  }
        };
        Assert.That(
            () => Goal.CheckAddActionsPreconditions(bgsData.First(), bgsData, conflicts),
            Throws.ArgumentException.And.Property("Message").EqualTo("All minor factions in systemConflicts must be in systemPresences"));
    }

    [TestCaseSource(nameof(GetControllingMinorFaction_Source))]
    public Presence GetControllingMinorFaction(IReadOnlySet<Presence> systemBgsData)
    {
        return Goal.GetControllingPresence(systemBgsData);
    }

    public static IEnumerable<TestCaseData> GetControllingMinorFaction_Source()
    {
        StarSystem betelgeuse = new() { Name = "Betelgeuse" };
        MinorFaction gumChewers = new() { Name = "Gum Chewers" };
        MinorFaction funnyWalkers = new() { Name = "Funny Walkers" };
        MinorFaction bunnyHoppers = new() { Name = "Bunny Hoppoers" };
        Presence gumChewersInBetegeuse = new() { StarSystem = betelgeuse, MinorFaction = gumChewers, Influence = 0.1 };
        Presence funnyWalkersInBetegeuse = new() { StarSystem = betelgeuse, MinorFaction = funnyWalkers, Influence = 0.3 };
        Presence bunnyHoppersInBetegeuse = new() { StarSystem = betelgeuse, MinorFaction = bunnyHoppers, Influence = 0.5 };

        return new TestCaseData[]
        {
            new TestCaseData(new HashSet<Presence>()
            {
                gumChewersInBetegeuse
            }).Returns(gumChewersInBetegeuse),
            new TestCaseData(new HashSet<Presence>()
            {
                gumChewersInBetegeuse,
                funnyWalkersInBetegeuse
            }).Returns(funnyWalkersInBetegeuse),
            new TestCaseData(new HashSet<Presence>()
            {
                gumChewersInBetegeuse,
                funnyWalkersInBetegeuse,
                bunnyHoppersInBetegeuse
            }).Returns(bunnyHoppersInBetegeuse)
        };
    }
}
