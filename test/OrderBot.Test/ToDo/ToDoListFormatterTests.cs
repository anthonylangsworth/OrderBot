﻿using NUnit.Framework;
using OrderBot.Core;
using OrderBot.Test.Samples;
using OrderBot.ToDo;

namespace OrderBot.Test.ToDo;

internal class ToDoListFormatterTests
{
    [Test]
    public void Format_Empty()
    {
        ToDoList toDoList = new("The Dark Wheel");
        Assert.That(new ToDoListFormatter().Format(toDoList), Is.EqualTo(
@"---------------------------------------------------------------------------------------------------------------------------------
***Pro-The Dark Wheel** support required* - Work for *The Dark Wheel* in these systems.
E.g. Missions/PAX, cartographic data, bounties, and profitable trade to *The Dark Wheel* controlled stations.
(None)

***Anti-The Dark Wheel** support required* - Work ONLY for the other factions in the listed systems to bring *The Dark Wheel*'s INF back to manageable levels and to avoid an unwanted expansion.
(None)

***Urgent Pro-Non-Native/Coalition Faction** support required* - Work for ONLY the listed factions in the listed systems to avoid a retreat or to disrupt system interference.
(None)

---------------------------------------------------------------------------------------------------------------------------------
**War Systems**
(None)

**Election Systems**
(None)
"));
    }

    /*
    Redeem bounty vouchers to increase security in systems *The Dark Wheel* controls.
    (None)
    */

    [Test]
    public void Format_ProAndAnti()
    {
        MinorFaction theDarkWheel = new() { Name = MinorFactionNames.DarkWheel };
        ToDoList toDoList = new(theDarkWheel.Name);
        toDoList.Suggestions.UnionWith(
            new Suggestion[]
            {
                    new InfluenceSuggestion(new StarSystem() { Name = "Shinrarta Dezhra" }, theDarkWheel, true, 0.1),
                    new InfluenceSuggestion(new StarSystem() { Name = "Tau Ceti" }, theDarkWheel, true, 0.2),
                    new InfluenceSuggestion(new StarSystem() { Name = "Wolf 359" }, theDarkWheel, false, 0.7),
                    new InfluenceSuggestion(new StarSystem() { Name = "Alpha Centauri" }, theDarkWheel, false, 0.65),
                    new SecuritySuggestion(new StarSystem() { Name = "Maia" }, SecurityLevel.Low )
            });
        Assert.That(new ToDoListFormatter().Format(toDoList), Is.EqualTo(
@"---------------------------------------------------------------------------------------------------------------------------------
***Pro-The Dark Wheel** support required* - Work for *The Dark Wheel* in these systems.
E.g. Missions/PAX, cartographic data, bounties, and profitable trade to *The Dark Wheel* controlled stations.
- Shinrarta Dezhra - 10%
- Tau Ceti - 20%

***Anti-The Dark Wheel** support required* - Work ONLY for the other factions in the listed systems to bring *The Dark Wheel*'s INF back to manageable levels and to avoid an unwanted expansion.
- Wolf 359 - 70%
- Alpha Centauri - 65%

***Urgent Pro-Non-Native/Coalition Faction** support required* - Work for ONLY the listed factions in the listed systems to avoid a retreat or to disrupt system interference.
(None)

---------------------------------------------------------------------------------------------------------------------------------
**War Systems**
(None)

**Election Systems**
(None)
"));

        /*- [Shinrarta Dezhra](<https://inara.cz/elite/search/?search=Shinrarta+Dezhra>) - 10%
        - [Tau Ceti](<https://inara.cz/elite/search/?search=Tau+Ceti>) - 20%

        Redeem bounty vouchers to increase security in systems *The Dark Wheel* controls.
        - [Maia](<https://inara.cz/elite/search/?search=Maia>) - Low

        ***Anti-The Dark Wheel** support required* - Work ONLY for the other factions in the listed systems to bring *The Dark Wheel*'s INF back to manageable levels and to avoid an unwanted expansion.
        - [Wolf 359](<https://inara.cz/elite/search/?search=Wolf+359>) - 70%
        - [Alpha Centauri](<https://inara.cz/elite/search/?search=Alpha+Centauri>) - 65%
        */

        /*
        Redeem bounty vouchers to increase security in systems *The Dark Wheel* controls.
        - Maia - Low
        */
    }
}
