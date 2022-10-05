﻿using NUnit.Framework;
using OrderBot.Core;
using OrderBot.Reports;

namespace OrderBot.Test.Reports
{
    internal class TestToDoListFormatter
    {
        [Test]
        public void Format_Empty()
        {
            ToDoList toDoList = new ToDoList("The Dark Wheel");
            Assert.That(new ToDoListFormatter().Format(toDoList), Is.EqualTo(
@"---------------------------------------------------------------------------------------------------------------------------------
***Pro-The Dark Wheel** support required* - Work for EDA in these systems.
Missions/PAX, Cartographic Data, Bounties, and Profitable Trade to EDA owned stations:
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

        [Test]
        public void Format_OneProAndAnti()
        {
            ToDoList toDoList = new ToDoList("The Dark Wheel");
            toDoList.Pro.Add(new InfluenceInitiatedAction() { StarSystem = new StarSystem() { Name = "Shinrarta Dezhra" }, Influence = 0.1 });
            toDoList.Anti.Add(new InfluenceInitiatedAction() { StarSystem = new StarSystem() { Name = "Wolf 359" }, Influence = 0.7 });
            Assert.That(new ToDoListFormatter().Format(toDoList), Is.EqualTo(
@"---------------------------------------------------------------------------------------------------------------------------------
***Pro-The Dark Wheel** support required* - Work for EDA in these systems.
Missions/PAX, Cartographic Data, Bounties, and Profitable Trade to EDA owned stations:
- Shinrarta Dezhra - 10%

***Anti-The Dark Wheel** support required* - Work ONLY for the other factions in the listed systems to bring *The Dark Wheel*'s INF back to manageable levels and to avoid an unwanted expansion.
- Wolf 359 - 70%

***Urgent Pro-Non-Native/Coalition Faction** support required* - Work for ONLY the listed factions in the listed systems to avoid a retreat or to disrupt system interference.
(None)

---------------------------------------------------------------------------------------------------------------------------------
**War Systems**
(None)

**Election Systems**
(None)
"));
        }
    }
}
