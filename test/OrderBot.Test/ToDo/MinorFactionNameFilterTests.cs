using NUnit.Framework;
using OrderBot.ToDo;

namespace OrderBot.Test.ToDo;

internal class MinorFactionNameFilterTests
{
    [TestCase()]
    [TestCase("")]
    [TestCase("", "a")]
    public void Ctor(params string[] minorFactions)
    {
        FixedMinorFactionNameFilter fixedMinorFactionNameFilter = new(minorFactions);
        Assert.AreEqual(fixedMinorFactionNameFilter.MinorFactions, minorFactions);
    }

    [TestCase(new[] { "a" }, "a", true)]
    [TestCase(new[] { "a" }, "b", false)]
    public void Match(IEnumerable<string> minorFactions, string name, bool expectedMatch)
    {
        FixedMinorFactionNameFilter fixedMinorFactionNameFilter = new(minorFactions);
        Assert.AreEqual(fixedMinorFactionNameFilter.Matches(name), expectedMatch);
    }
}
