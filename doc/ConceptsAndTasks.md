# Tasks

## Setup

NOTE: This Discord Bot is **NOT** currently open to joining other Discord servers. It is still a work in progress and some values are hard coded.

To start, you need an accound in the Discord server with the following permissions:
1. `Manage Server`: Required to add the bot to the server.
2. `Manage Channels` and `Manage Roles`: This bot's commands can only run by a user with these permissions by default. However, the bot's commands can be delegated to other roles. See [Security](Security.md) for details. 

To setup the bot:
1. Add the bot to your Discord server by following this [link](https://discordapp.com/oauth2/authorize?client_id=1032610954325790760&scope=bot&permissions=0).
2. Give Discord a few minutes to process the registration and the bot's commands to become available. 
3. Choose a minor faction to support using [/todo-list support set](CommandReference.md#todo-list-support-set), such as the one associated with an in-game squadron. The name entered must match the name of the in-game minor faction *exactly*, although it is not case sensitive.
4. Either (1) visit the systems where your supported minor faction is present while running [EDMC](https://github.com/EDCD/EDMarketConnector/wiki) or a similar tool or (2) wait for someone else to do so. This sends data to this bot.
5. Run [/todo-list show](CommandReference.md#todo-list-show) to see the suggestions. These suggestions update automatically over time as new data comes in.

Optional, additional tasks:
1. Override the behaviour for specific systems or add goals for other minor factions using [/todo-list goal add](CommandReference.md#todo-list-goal-add). See [Goals](Goals.md) for details. You can add or remove these at any time as needs or whims dictate.
2. Set an channel where fleet carrier movements are logged to using [/carrier-movement channel set](CommandReference.md#carrier-movement-channel-set). Ignore your squadron or trusted carriers using [/carrier-movement ignored-carriers add](CommandReference.md#carrier-movement-ignored-carriers-add).
3. Allow other people on the Discord server to see the todo list or configure the bot by adding them to the bot roles. See [Security](Security.md) for details.
4. Set an audit channel where changes are logged to using [/bgs-order-bot audit-channel set](CommandReference.md#bgs-order-bot-audit-channel-set). See [Security](Security.md) for more details.

## See BGS Work

Use [/todo-list show](CommandReference.md#todo-list-show) to see a list of suggested BGS work. The result is ephemeral, only visible to the calling user, as with all commands for this bot. Specify goals using [/todo-list goal add](CommandReference.md#todo-list-goal-add) to further refine the suggestions.

To get a version you can copy, edit and post in a Discord channel without losing formatting, use [/todo-list raw](CommandReference.md#todo-list-raw). 

## Backup, Bulk Edit and Restore Settings

Periodically, after setup or after major changes, export configuration data and save it in a safe place. Save the output of the following commands:
1. [/todo-list support get](CommandReference.md#todo-list-support-get)
2. [/todo-list goal export](CommandReference.md#todo-list-goal-export)
3. [/carrier-movement ignored-carriers export](CommandReference.md#carrier-movement-ignored-carriers-export)

You can edit the CSV files in a text editor or spreadsheet program to add details in bulk.

You can them import the data using 
1. [/todo-list support set](CommandReference.md#todo-list-support-set)
2. [/todo-list goal import](CommandReference.md#todo-list-goal-import)
3. [/carrier-movement ignored-carriers import](CommandReference.md#carrier-movement-ignored-carriers-import)

# Concepts

This bot has two ways of specifying how to interact with the BGS. It can also monitor incoming fleet carriers.

## Support

The first is "supporting" a minor faction (see [/todo-list support set](CommandReference.md#todo-list-support-set)). This does three things:
1. The bot monitors BGS activity in star systems where this minor faction has a presence. It does not monitor any systems where supported minor factions are not present.
2. Creates an implicit 'Control' goal for the minor faction in each system. See below for details on goals. This can be overridden in individual star sytems (see [/todo-list goal add](CommandReference.md#todo-list-goal-add)).
3. Monitors carrier movements, writing the details to the specified carrier movement channel (see [/carrier-movement channel set](CommandReference.md#carrier-movement-channel-set)).

## Goals

By default, no minor factions are supported. Each must be manually added.

The second is by "goals" (see [/todo-list goal add](CommandReference.md@todo-list-goal-add)). A goal is a specific instruction, intention or aim for a minor faction in a star system, described in [Goals](Goals.md). They can be used for many purposes, such as:
1. Overide the implicit `Control` goal for a supported minor faction in a star system, such as `Ignoring` systems expanded into by accident.
2. Work to expand from a star system using the `Expand` goal.
3. Work to retreat a non-native minor faction using the `Retreat` goal.
4. Prevent a non-native minor faction from retreating using the `Maintain` goal.

Like support, no goals are specified by default. Each must be manually added.

## Carrier Movements

The bot also monitors fleet carriers jumping into systems. This can be a good indication of commanders from other squadrons acting against you, such as by "hydrogen bombing". Specify the channel to write alerts to using [/carrier-movement channel set](CommandReference.md#carrier-movement-channel-set). The bot needs `Send Messages` permission to the channel.

The bot also supports a list of fleet carriers to ignore (see [/carrier-movement ignored-carriers add](CommandReference.md#carrier-movement-ignored-carriers-add)). This stops the bot from reporting the movement of friendly or known carriers.

# Limitations

This bot depends on data from the Elite Dangerous Data Network (EDDN). This is the same place where Inara, EDDB, EDSM and similar websites source their data. This means the bot is only as accurate as the data it receives from EDDN. This means:
1. **Elite Dangerous** players not running EDMC or a similar program while playing will not report their data to EDDN. EDDN and, therefore. this bot, will never see this data.
2. BGS data changes on daily and weekly ticks. If players have not visited the relevant systems since the last tick, the data the bot uses to determine suggestions will be out of date.
3. EDMC and similar programs get data from the client's journal file. Sometimes the data in the client's journal file does not match that in-game or takes several hours to catch up. The bot's data will be out of date or incorrect until the journal matches the in-game data.
