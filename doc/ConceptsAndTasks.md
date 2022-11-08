# Concepts

(Outline. TODO: Fill out.)

Support:
- Does three things
  - Minors BGS activity where this minor faction has a presence
  - Creates an implicit 'Control' for the minor faction in each system
  - Monitors carrier movements, writing the details to 
- Control goal can be overridden for each system
- By default, no minor factions are supported. Each must be manually added.

Goal:
- A goal is a specific instruction, intention or aim for a minor faction in a star system.
- Can be used to:
  - Overide the implicit `Control` goal for a supported minor faction in a star system, such as `Ignoring` systems expanded into by accident
  - Work to retreat a minor faction using the `Retreat` goal
  - Prevent a non-native minor faction from retreating using the `Maintain` goal
- No goals are specified by default

Limitations
- Cannot see data if users are not running 

# Tasks

## Setup

See [README](../README.md#setup) for instructions on setting up the bot.

## Backup and Restore Settings

Periodically, after setup or after major changes, export configuration data and save it in a safe place. Save the output of the following commands:
1. `/todo-list support list`
2. `/todo-list goal export`
3. `/carrier-movement ignored-carrers export`
