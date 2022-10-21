using NUnit.Framework;
using OrderBot.Core;
using OrderBot.ToDo;

namespace OrderBot.Test.ToDo
{
    internal class TestToDoListFormatter
    {
        [Test]
        public void Format_Empty()
        {
            ToDoList toDoList = new("The Dark Wheel");
            Assert.That(new ToDoListFormatter().Format(toDoList), Is.EqualTo(
@"---------------------------------------------------------------------------------------------------------------------------------
***Pro-The Dark Wheel** support required* - Work for EDA in these systems.
Missions/PAX, Cartographic Data, Bounties, and Profitable Trade to *The Dark Wheel* controlled stations:
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
        public void Format_ProAndAnti()
        {
            ToDoList toDoList = new("The Dark Wheel");
            toDoList.Pro.Add(new InfluenceInitiatedAction() { StarSystem = new StarSystem() { Name = "Shinrarta Dezhra" }, Influence = 0.1 });
            toDoList.Pro.Add(new InfluenceInitiatedAction() { StarSystem = new StarSystem() { Name = "Tau Ceti" }, Influence = 0.2 });
            toDoList.Anti.Add(new InfluenceInitiatedAction() { StarSystem = new StarSystem() { Name = "Wolf 359" }, Influence = 0.7 });
            toDoList.Anti.Add(new InfluenceInitiatedAction() { StarSystem = new StarSystem() { Name = "Alpha Centauri" }, Influence = 0.65 });
            Assert.That(new ToDoListFormatter().Format(toDoList), Is.EqualTo(
@"---------------------------------------------------------------------------------------------------------------------------------
***Pro-The Dark Wheel** support required* - Work for EDA in these systems.
Missions/PAX, Cartographic Data, Bounties, and Profitable Trade to *The Dark Wheel* controlled stations:
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
        }
    }
}
