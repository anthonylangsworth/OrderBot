﻿using NUnit.Framework;
using OrderBot.Core;
using OrderBot.ToDo;

namespace OrderBot.Test.ToDo;

internal class FightTests
{
    [Test]
    [TestCaseSource(nameof(For_Source))]
    public ConflictSuggestion? For(MinorFaction minorFaction, Conflict conflict)
        => Fight.For(minorFaction, conflict);

    public static IEnumerable<TestCaseData> For_Source()
    {
        StarSystem starSystem = new() { Name = "T Tauri" };
        MinorFaction blueSparrows = new() { Name = "Blue Sparrows" };
        MinorFaction redEagles = new() { Name = "Red Eagles" };
        MinorFaction yellowParrots = new() { Name = "Yellow Parrots" };
        Conflict blueVsRed = new()
        {
            MinorFaction1 = blueSparrows,
            MinorFaction1WonDays = 1,
            MinorFaction2 = redEagles,
            MinorFaction2WonDays = 2,
            StarSystem = starSystem,
            Status = ConflictStatus.Active,
            WarType = WarType.Election
        };

        return new TestCaseData[]
        {
            new TestCaseData(blueSparrows, blueVsRed).Returns(
                new ConflictSuggestion(starSystem, blueSparrows, blueVsRed.MinorFaction1WonDays,
                    redEagles, blueVsRed.MinorFaction2WonDays, ConflictState.CloseDefeat, WarType.Election)),
            new TestCaseData(redEagles, blueVsRed).Returns(
                new ConflictSuggestion(starSystem, redEagles, blueVsRed.MinorFaction2WonDays,
                    blueSparrows, blueVsRed.MinorFaction1WonDays, ConflictState.CloseVictory, WarType.Election)),
            new TestCaseData(yellowParrots, blueVsRed).Returns(null)
        };
    }

    [Test]
    [TestCaseSource(nameof(Against_Source))]
    public ConflictSuggestion? Against(MinorFaction minorFaction, Conflict conflict)
        => Fight.Against(minorFaction, conflict);

    public static IEnumerable<TestCaseData> Against_Source()
    {
        StarSystem starSystem = new() { Name = "T Tauri" };
        MinorFaction blueSparrows = new() { Name = "Blue Sparrows" };
        MinorFaction redEagles = new() { Name = "Red Eagles" };
        MinorFaction yellowParrots = new() { Name = "Yellow Parrots" };
        Conflict blueVsRed = new()
        {
            MinorFaction1 = blueSparrows,
            MinorFaction1WonDays = 1,
            MinorFaction2 = redEagles,
            MinorFaction2WonDays = 3,
            StarSystem = starSystem,
            Status = ConflictStatus.Active,
            WarType = WarType.War
        };

        return new TestCaseData[]
        {
            new TestCaseData(blueSparrows, blueVsRed).Returns(
                new ConflictSuggestion(starSystem, redEagles, blueVsRed.MinorFaction2WonDays,
                    blueSparrows, blueVsRed.MinorFaction1WonDays, ConflictState.Victory, WarType.War)),
            new TestCaseData(redEagles, blueVsRed).Returns(
                new ConflictSuggestion(starSystem, blueSparrows, blueVsRed.MinorFaction1WonDays,
                    redEagles, blueVsRed.MinorFaction2WonDays, ConflictState.Defeat, WarType.War)),
            new TestCaseData(yellowParrots, blueVsRed).Returns(null)
        };
    }

    [Test]
    [TestCaseSource(nameof(Between_Source))]
    public ConflictSuggestion? Between(MinorFaction fightFor, MinorFaction fightAgainst, Conflict conflict)
        => Fight.Between(fightFor, fightAgainst, conflict);

    public static IEnumerable<TestCaseData> Between_Source()
    {
        StarSystem starSystem = new() { Name = "T Tauri" };
        MinorFaction blueSparrows = new() { Name = "Blue Sparrows" };
        MinorFaction redEagles = new() { Name = "Red Eagles" };
        MinorFaction yellowParrots = new() { Name = "Yellow Parrots" };
        Conflict blueVsRed = new()
        {
            MinorFaction1 = blueSparrows,
            MinorFaction1WonDays = 1,
            MinorFaction2 = redEagles,
            MinorFaction2WonDays = 2,
            StarSystem = starSystem,
            Status = ConflictStatus.Active,
            WarType = WarType.CivilWar
        };

        return new TestCaseData[]
        {
            new TestCaseData(blueSparrows, redEagles, blueVsRed).Returns(
                new ConflictSuggestion(starSystem, blueSparrows, blueVsRed.MinorFaction1WonDays,
                    redEagles, blueVsRed.MinorFaction2WonDays, ConflictState.CloseDefeat, WarType.CivilWar)),
            new TestCaseData(redEagles, blueSparrows, blueVsRed).Returns(
                new ConflictSuggestion(starSystem, redEagles, blueVsRed.MinorFaction2WonDays,
                    blueSparrows, blueVsRed.MinorFaction1WonDays, ConflictState.CloseVictory, WarType.CivilWar)),
            new TestCaseData(blueSparrows, yellowParrots, blueVsRed).Returns(null),
            new TestCaseData(yellowParrots, blueSparrows, blueVsRed).Returns(null),
            new TestCaseData(yellowParrots, redEagles, blueVsRed).Returns(null),
            new TestCaseData(redEagles, yellowParrots, blueVsRed).Returns(null),
        };
    }
}
