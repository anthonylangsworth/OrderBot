using NUnit.Framework;
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

***Pro-Non-Native/Coalition Faction** support required* - Work for ONLY the listed factions in the listed systems to avoid a retreat or to disrupt system interference.
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
    public void Format_ProAntiAndConflicts()
    {
        MinorFaction axi = new() { Name = MinorFactionNames.AXI };
        MinorFaction operationIda = new() { Name = MinorFactionNames.OperationIda };
        MinorFaction antHillMob = new() { Name = "The Ant Hill Mob" };
        StarSystem maia = new() { Name = StarSystemNames.Maia };
        StarSystem celaeno = new() { Name = StarSystemNames.Celaeno };
        StarSystem merope = new() { Name = StarSystemNames.Merope };
        StarSystem atlas = new() { Name = StarSystemNames.Atlas };
        StarSystem asterope = new() { Name = StarSystemNames.Asterope };
        StarSystem pleione = new() { Name = StarSystemNames.Pleione };
        StarSystem electra = new() { Name = StarSystemNames.Electra };

        ToDoList toDoList = new(axi.Name);
        toDoList.Suggestions.UnionWith(
            new Suggestion[]
            {
                new InfluenceSuggestion(maia, axi, true, 0.1),
                new InfluenceSuggestion(celaeno, axi, true, 0.2),
                new InfluenceSuggestion(merope, axi, false, 0.7),
                new InfluenceSuggestion(atlas, axi, false, 0.65),
                new InfluenceSuggestion(asterope, axi, true, 0.05),
                new ConflictSuggestion(pleione, axi, 2, antHillMob, 1, ConflictState.CloseVictory, WarType.War),
                new ConflictSuggestion(electra, axi, 1, antHillMob, 3, ConflictState.Defeat, WarType.War),
                new InfluenceSuggestion(merope, operationIda, true, 0.04)
            });
        Assert.That(new ToDoListFormatter().Format(toDoList), Is.EqualTo(
@"---------------------------------------------------------------------------------------------------------------------------------
***Pro-Anti Xeno Initiative** support required* - Work for *Anti Xeno Initiative* in these systems.
E.g. Missions/PAX, cartographic data, bounties, and profitable trade to *Anti Xeno Initiative* controlled stations.
- Asterope - 5%
- Maia - 10%
- Celaeno - 20%

***Anti-Anti Xeno Initiative** support required* - Work ONLY for the other factions in the listed systems to bring *Anti Xeno Initiative*'s INF back to manageable levels and to avoid an unwanted expansion.
- Merope - 70%
- Atlas - 65%

***Pro-Non-Native/Coalition Faction** support required* - Work for ONLY the listed factions in the listed systems to avoid a retreat or to disrupt system interference.
- *Operation Ida* in Merope - 4%

---------------------------------------------------------------------------------------------------------------------------------
**War Systems**
- Electra - Fight for *Anti Xeno Initiative* against *The Ant Hill Mob* - 1 vs 3 (*Defeat*)
- Pleione - Fight for *Anti Xeno Initiative* against *The Ant Hill Mob* - 2 vs 1 (*Close Victory*)

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
