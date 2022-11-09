# Command Reference

Command are listed alphabetically within the two main functional areas: [To-Do List](#To-Do-List) and [Carrier Movement](#Carrier-Movement). To understand the concepts and common tasks, giving these commands context, see [Concepts and Tasks](ConceptsAndTasks.md).

All commands can be run by users that can manage channels and users. In the future, it will be possible to also allow specific users or roles access to commands.

The results of all commands are ephemeral, only shown to the calling user. 

Command details, particularly validation and autocomplate, are a work in progress. Some commands may write details to an audit channel or the Discord server's audit log in the future.

## To Do List

### /todo-list goal add

Set a [goal](Goals.md) for a minor faction in a star system. This overrides the implicit `Control` [goal](Goals.md) for a supported minor faction (see [/todo-list support add](#todo-list-support-add)). There are no goals for any system or minor faction by default.

To change a goal, use this command again, specifying the same star system and minor faction but the new goal. To remove any goal for a star system and minor faction, use [/todo-list goal remove](#todo-list-goal-remove).

The bot writes a short description of the goal to the audit channel.

Parameters:

|name|Description|Validation|Autocomplete|
|----|-----------|----------|------------|
|minor-faction|The name of the minor faction to support|Must be a minor system already in the database.|None|
|star-system|The name of the star system|Must be a star system already in the database.|None|
|goal|The name of the goal|Must be a valid goal.|Known goals.|

### /todo-list goal export

Export the set goals as a CSV (command separated variable) file. You can view or edit this file in a text editor or common spreadsheet programs like **Microsoft Excel**. This allows easy backup or transfer of the goals that can later be imported using [/todo-list goal import](#todo-list-goal-import).

### /todo-list goal import

Import a list of set goals as a CSV (command separated variable) file, adding (not replacing) to the existing goals. This allows easy backup or transfer of the goals previously exported using [/todo-list goal export](#todo-list-goal-export). 

Existing goals are NOT cleared first. This is additive.

The bot writes a short description of each goal imported to the audit channel.

### /todo-list goal list

Similar to [/todo-list goal export](#todo-list-goal-export), this shows a list of set goals in a more human readable format. This helps see what goals are set.

### /todo-list goal remove

Remove any goal added by [/todo-list goal add](#todo-list-goal-add) for a minor faction in a star system. 

The bot writes a short description of the removed goal to the audit channel.

Parameters:

|name|Description|Validation|Autocomplete|
|----|-----------|----------|------------|
|minor-faction|The name of the minor faction to support|Must be a minor faction already used with a goal.|None|
|star-system|The name of the star system|Must be a star system already used with a goal.|None|

### /todo-list raw

Similar to [/todo-list show](#todo-list-show), this outputs a list of suggested in-game activities. Unlike `/todo-list show`, this escapes the markdown formatting, allowing easy copying and pasting into another Discord channel to preserve the formatting. This command is intended for squadron admins that want to use this bot as an aid, customizing its output before posting it.

**NOTE**: This is currently hard-coded to a specific minor faction.

### /todo-list show

Show a list of suggested in-game activites. This can be run by (1) squadron admins to help create daily suggested activities to support a minor faction or (2) squadron members themselves if admins are happy to delegate the work to this bot. To get a version that admins can edit before pasting into a Discord channel without losing formatting, use [/todo-list raw](#todo-list-raw).

**NOTE**: This is currently hard-coded to a specific minor faction.

### /todo-list support add

Support a minor faction. For each system where this minor faction is present, this (1) instructs the bot to listen for BGS information, (2) adds an implicit `Control` [goal](Goals.md) for that minor faction and (3) writes details of carrier's jumping in to the carrier movement channel. Override this implicit goal for specific star systems using [/todo-list goal add](#todo-list-goal-add). Use [/todo-list support remove](#todo-list-support-remove) to stop supporting a minor faction.

The bot writes a short description of the supported minor faction.

Parameters:

|name|Description|Validation|Autocomplete|
|----|-----------|----------|------------|
|minor-faction|The name of the minor faction to support.|None|None|

### /todo-list support list

Show a list of supported minor factions.

### /todo-list support remove

Stop supporting a minor faction previously supported using [/todo-list support add](#todo-list-support-add). 

The bot writes a short description of the removed minor faction.

Parameters:

|name|Description|Validation|Autocomplete|
|----|-----------|----------|------------|
|minor-faction|The name of the minor faction to stop supporting.|Must be a supported minor faction.|None|

## Carrier Movement

### /carrier-movement channel clear

Stop writting carrier movements to any channel. 

### /carrier-movement channel get

Show the channel where this bot writes carrier movements previously set using [/carrier-movement channel set](#carrier-movement-channel-set).

### /carrier-movement channel set

Set the channel where this bot writes carrier movements. This bot must have `Send Messages` access to the channel. No channel is set by default so you must use this command to set a channel before the bot will report carriers jumping in. 

Use this command again, specifying a different channel, to change the channel where this bot writes carrier commands. 

Parameters:

|name|Description|Validation|Autocomplete|
|----|-----------|----------|------------|
|channel|The channel.|Must be a channel on this server.|A list of channels on this server.|

### /carrier-movement ignored-carriers add

Ignore a fleet carrier. This prevents the bot reporting when it jumps into a system where a supported minor faction is present. This stops the bot from reporting on friendly or squadron carriers.

To remove a carrier from the ignore list and report its movements, use [/carrier-movement ignored-carriers remove](#carrier-movement-ignored-carriers-remove).

The bot writes a short description of the ignored carrier to the audit log.

Parameters:

|name|Description|Validation|Autocomplete|
|----|-----------|----------|------------|
|carrier-name|The name of the carrier to ignore.|A valid carrier name, ending with the seven character serial number.|Carriers that are not ignored.|

### /carrier-movement ignored-carriers export

Export the list of ignored fleet carrers as a CSV (command separated variable) file. You can view or edit this file in a text editor or common spreadsheet programs like **Microsoft Excel**. This allows easy backup or transfer of the goals that can later be imported using [/carrier-movement ignored-carriers import](#carrier-movement-ignored-carriers-import).

### /carrier-movement ignored-carriers import

Import a list of ignored fleet carriers as a CSV (command separated variable) file, adding to (not replacing) the ignored carriers. This allows easy backup or transfer of the goals previously exported using [/carrier-movement ignored-carriers export](#carrier-movement-ignored-carriers-export).

The bot writes a short description of each ignored carrier to the audit log.

Existing ignored carriers are NOT cleared first. This is additive.

### /carrier-movement ignored-carriers list

Similar to [/carrier-movement ignored-carriers export](#carrier-movement-ignored-carriers-export), this shows a list of ignored fleet carriers in a more human readable format. This helps see what carriers are ignored or whether a specific carrier is on the list.

### /carrier-movement ignored-carriers remove

Track a fleet carrier, reporting when it jumps into a system where a supported minor faction is present. All carriers are tracked by default. To remove it from the ignore list and report its movements, use [/carrier-movement ignored-carriers remove](#carrier-movement-ignored-carriers-remove).

The bot writes a short description of the tracked carrier to the audit log.

Parameters:

|name|Description|Validation|Autocomplete|
|----|-----------|----------|------------|
|carrier-name|The name of an ignored carrier.|A valid carrier name, ending with the seven character serial number.|Ignored carriers.|

