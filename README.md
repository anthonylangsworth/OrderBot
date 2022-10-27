[![Build and Test](https://github.com/anthonylangsworth/OrderBot/actions/workflows/main.yml/badge.svg)](https://github.com/anthonylangsworth/OrderBot/actions/workflows/main.yml)
[![Deploy](https://github.com/anthonylangsworth/OrderBot/actions/workflows/deploy.yml/badge.svg)](https://github.com/anthonylangsworth/OrderBot/actions/workflows/deploy.yml)

# BGS Order Bot

## Overview
For any squadron in the game *Elite Dangerous* that supports a minor faction, keeping up to date with the background simulation (BGS) is an onerous task. For example, player activity and BGS randomness can reduce or increase influence, leading to conflicts, expansions or retreats. Trawling through each system on [Inara](https://inara.cz/) is time-consuming and error prone.

This bot receives BGS data from the [Elite Dangerous Data Network](https://eddn.edcd.io/) and creates a list of suggestions based on an admin-specified minor faction to support. This allows the bot to produce specific suggestions, unlike Inara or [Elite BGS App](https://elitebgs.app/).

This project is not intended to replace squadron officers, although it can be used that way, or existing tools like those mentioned above. For example, it will not give graphs of influence history.

## Setup
NOTE: This Discord Bot is **NOT** currently open to joining other Discord servers. It is still a work in progress and some values are hard coded.

To quickly setup the bot:
1. Add the bot to your Discord server by <URL Not Available Yet>.
2. Give Discord a few minutes to process the registration and the bot's commands to become available. 
3. Choose a minor faction to support using `/todo-list support add <minor faction name>` where `<minor faction name>` is the name of the minor faction you want to support. This must match the name of the in-game squadron exactly, although it is not case sensitive.
4. Override the behaviour for specific systems or add goals for other minor factions using `/todo-list goal add <minor faction> <star system> <goal>`. See [Goals](doc/Goals.md) for details. You can add or remove these at any time as needs or whims dictate.
5. Either visit the systems where your supported minor faction is present while running [EMDC](https://github.com/EDCD/EDMarketConnector/wiki) or a similar tool or wait for someone else to do so. This sends data to EDDN and then on to this bot.
6. Run `/todo-list show` to see the suggestions.

## Sample Suggestions

(The structure is still a work in progress.)
  
> ---------------------------------------------------------------------------------------------------------------------------------
> ***Pro-The Dark Wheel** support required* - Work for EDA in these systems.
> E.g. Missions/PAX, cartographic data, bounties, and profitable trade to *The Dark Wheel* controlled stations.
> - [Shinrarta Dezhra](<https://inara.cz/elite/search/?search=Shinrarta+Dezhra>) - 10%
> - [Tau Ceti](<https://inara.cz/elite/search/?search=Tau+Ceti>) - 20%
> 
> Redeem bounty vouchers to increase security in systems *The Dark Wheel* controls.
> - [Maia](<https://inara.cz/elite/search/?search=Maia>) - Low
> 
> ***Anti-The Dark Wheel** support required* - Work ONLY for the other factions in the listed systems to bring *The Dark Wheel*'s INF back to manageable levels and to > avoid an unwanted expansion.
> - [Wolf 359](<https://inara.cz/elite/search/?search=Wolf+359>) - 70%
> - [Alpha Centauri](<https://inara.cz/elite/search/?search=Alpha+Centauri>) - 65%
> 
> ***Urgent Pro-Non-Native/Coalition Faction** support required* - Work for ONLY the listed factions in the listed systems to avoid a retreat or to disrupt system interference.
> (None)
> 
> ---------------------------------------------------------------------------------------------------------------------------------
> **War Systems**
> (None)
> 
> **Election Systems**
> (None)

## Development and Contributions

See [Development](doc/Development.md).
