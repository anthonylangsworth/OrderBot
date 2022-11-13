[![Build and Test](https://github.com/anthonylangsworth/OrderBot/actions/workflows/main.yml/badge.svg)](https://github.com/anthonylangsworth/OrderBot/actions/workflows/main.yml)
[![Deploy](https://github.com/anthonylangsworth/OrderBot/actions/workflows/deploy.yml/badge.svg)](https://github.com/anthonylangsworth/OrderBot/actions/workflows/deploy.yml)

# BGS Order Bot

## Overview
For any squadron in the game **Elite Dangerous** that supports a minor faction, keeping up to date with the background simulation (BGS) is an onerous task. For example, player activity and BGS randomness can reduce or increase influence, leading to conflicts, expansions or retreats. Trawling through each system on websites like [Inara](https://inara.cz/) or [Elite BGS](https://elitebgs.app/) is time-consuming and error prone.

This Discord bot aims to fix that. It receives BGS data from the [Elite Dangerous Data Network (EDDN)](https://eddn.edcd.io/) and creates a list of suggestions based on an admin-specified minor faction(s) to support and star system-specific goals. This allows the bot to produce specific and useful suggestions, unlike the websites mentioned above.

This Discord bot is not intended to replace squadron officers, although it can be used that way, or existing tools like those mentioned above. For example, it will not give graphs of influence history like Inara or Elite BGS. However, it can save time and increase accuracy by automating much of the work.

This Discord bot can send alerts to a Discord channel when unknown carriers jump into system where their supported minor faction is present. This may indicate when others are working against you.

## Sample Suggestions

Suggested BGS work includes explanations to help players understand relevant in-game actions. The example below is shown as markdown to prevent Github auto-formatting the output differently to Discord. 

The format is a work in progress so this may be out of date.

```
---------------------------------------------------------------------------------------------------------------------------------
***Pro-The Dark Wheel** support required* - Work for EDA in these systems.
E.g. Missions/PAX, cartographic data, bounties, and profitable trade to *The Dark Wheel* controlled stations.
- Shinrarta Dezhra - 10%
- Tau Ceti - 20%
 
Redeem bounty vouchers to increase security in systems *The Dark Wheel* controls.
- Maia - Low

***Anti-The Dark Wheel** support required* - Work ONLY for the other factions in the listed systems to bring *The Dark Wheel*'s INF back to manageable levels and to > avoid an unwanted expansion.
- Wolf 359 - 70%
- Alpha Centauri - 65%

***Urgent Pro-Non-Native/Coalition Faction** support required* - Work for ONLY the listed factions in the listed systems to avoid a retreat or to disrupt system interference.
(None)

---------------------------------------------------------------------------------------------------------------------------------
**War Systems**
(None)

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
