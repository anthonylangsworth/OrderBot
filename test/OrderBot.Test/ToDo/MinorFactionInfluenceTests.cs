using NUnit.Framework;
using OrderBot.ToDo;

namespace OrderBot.Test.ToDo;

internal class MinorFactionInfluenceTests
{
    [TestCaseSource(nameof(Ctor_Source))]
    public void Ctor(string minorFaction, double influence, IReadOnlyList<string> states)
    {
        EddnMinorFactionInfluence minorFactionInfluence = new()
        {
            MinorFaction = minorFaction,
            Influence = influence,
            States = states
        };
        Assert.That(minorFactionInfluence.MinorFaction, Is.EqualTo(minorFaction));
        Assert.That(minorFactionInfluence.Influence, Is.EqualTo(influence));
        Assert.That(minorFactionInfluence.States, Is.EquivalentTo(states));
    }

    public static IEnumerable<TestCaseData> Ctor_Source()
    {
        return new[]
        {
            new TestCaseData("a", 0.1, Array.Empty<string>()),
            new TestCaseData("b", 0.8, new string[] { "Boom", "Terrorist Attack" })
        };
    }
}
