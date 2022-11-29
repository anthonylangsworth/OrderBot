[![Build and Test](https://github.com/anthonylangsworth/OrderBot/actions/workflows/main.yml/badge.svg)](https://github.com/anthonylangsworth/OrderBot/actions/workflows/main.yml)
[![Deploy](https://github.com/anthonylangsworth/OrderBot/actions/workflows/deploy.yml/badge.svg)](https://github.com/anthonylangsworth/OrderBot/actions/workflows/deploy.yml)

# BGS Order Bot

## Overview
For any squadron in the game **Elite Dangerous** that supports a minor faction, keeping up to date with the background simulation (BGS) is an onerous task. For example, player activity and BGS randomness can reduce or increase influence, leading to conflicts, expansions or retreats.

Trawling through each system on websites like [Inara](https://inara.cz/) or [Elite BGS](https://elitebgs.app/) is time-consuming and error prone. Not monitoring these websites means missing a retreating minor faction or an unintended expansion into an ally's star system.

This Discord bot aims to fix that. It receives BGS data from the [Elite Dangerous Data Network (EDDN)](https://eddn.edcd.io/) and creates a list of suggestions based on an admin-specified minor faction(s) to support and star system-specific goals. This allows the bot to produce specific and useful suggestions, unlike the websites mentioned above.

This Discord bot is not intended to replace squadron officers, although it can be used that way, or existing tools like those mentioned above. For example, it will not give graphs of influence history like Inara or Elite BGS. However, it can save time and increase accuracy by automating much of the work.

This Discord bot can send alerts to a Discord channel when unknown carriers jump into system where their supported minor faction is present. This may indicate when others are working against you.

## Sample Suggestions

Suggested BGS work includes explanations to help players understand relevant in-game actions. The example below is shown as markdown to prevent Github auto-formatting the output differently to Discord. 

The format is a work in progress so this may be out of date. The format aims to clearly communicate what needs to be done to an average player based on the most recent data received from EDDN, the supported minor faction and any goals.

A (fictional) example of supporting the Anti Xeno Initiative, who is active in a few systems in the Pleiades, is given below. They are fighting in two wars and also helping *Operation Ida* in Merope. 

```markdown
---------------------------------------------------------------------------------------------------------------------------------
***Pro-Anti Xeno Initiative** support required* - Work for *Anti Xeno Initiative* in these systems.
E.g. Missions/PAX, cartographic data, bounties, and profitable trade to *Anti Xeno Initiative* controlled stations.
- Asterope - 5%
- Maia - 10%
- Celaeno - 20%

***Anti-Anti Xeno Initiative** support required* - Work ONLY for the other factions in the listed systems to bring *Anti Xeno Initiative*'s INF back to manageable levels and to avoid an unwanted expansion.
- Merope - 70%
- Atlas - 65%

***Urgent Pro-Non-Native/Coalition Faction** support required* - Work for ONLY the listed factions in the listed systems to avoid a retreat or to disrupt system interference.
- *Operation Ida* in Merope - 4%

---------------------------------------------------------------------------------------------------------------------------------
**War Systems**
- Electra - Fight for *Anti Xeno Initiative* against *The Ant Hill Mob* - 1 vs 3 (*Defeat*)
- Pleione - Fight for *Anti Xeno Initiative* against *The Ant Hill Mob* - 2 vs 1 (*Close Victory*)

**Election Systems**
(None)
```

## Use

See:
1. [Setup instructions](doc/ConceptsAndTasks.md#setup) to setup the bot in your Discord server. 
2. [Concepts and Tasks](doc/ConceptsAndTasks.md) for foundational concepts and common use cases. 
3. [Security](doc/Security.md) for details about access control and auditing.
4. [Command Reference](doc/CommandReference.md) about the commands available.

## Development and Contributions

See [Development](doc/Development.md).

## License

See [LICENSE](LICENSE) for the license (GPL v3).
