# Security

## Roles

This discord bot supports [role based access control (RBAC)](https://en.wikipedia.org/wiki/Role-based_access_control). It has two roles: Members, representing anyone in the squadron or who does background simulation (BGS) work, and Officers, those who set and control the squadron's tactics and BGS work. 

By default, these roles are empty. Add Discord roles to the bot roles via [/bgs-order-bot rbac add-member](CommandReference.md#bgs-order-bot-rbac-add-member).

Access to commands follows these principles:
1. Admins, users with `Manage Channels` and `Manage Roles`, can run all commands.
2. Channel setting and role management are restricted to Admins.
3. Officers can control the supported minor faction, goals and ignored carriers. Officers can do everything Members can.
4. Members can view the orders, the supported minor faction, goals and ignored carriers.

## Auditing

This discord bot can log changes to an audit channel. This helps when multiple Officers are controlling the BGS, allowing them to observe others' behaviour and prevent duplicate or conflicting work.

This discord bot requires `Send Messages` permission to the channel. Set the audit channel using [/bgs-order-bot audit-channel set](CommandReference.md#bgs-order-bot-audit-channel-set). The audit channel should be:
1. Read-only my non-admin users. That is, they should not have `Send Messages` or `Manage Messages` access to this channel to prevent accidental or deliberate deletion of audit messages.
2. Only visible to members of the `Officers` role and discord administrationrs. That is, only those roles should have `View Channel` permission. Adding emotes and other things are unecessary.

## Reference

A summary of access for each command is:

|Command|Members|Officers|Admins (Users with `Manage Channels` and `Manage Roles`)|Audited|
|-------|:-----:|:------:|:------------------------------------------------------:|:-----:|
|/bgs-order-bot audit-channel clear|❌|❌|✅|✅|
|/bgs-order-bot audit-channel get|❌|❌|✅|❌|
|/bgs-order-bot audit-channel set|❌|❌|✅|✅
|/bgs-order-bot rbac add-member|❌|❌|✅|✅|
|/bgs-order-bot rbac list|❌|❌|✅|❌|
|/bgs-order-bot rbac remove-member|❌|❌|✅|✅|
|/carrier-movement channel set|❌|❌|✅|✅|
|/carrier-movement channel get|❌|❌|✅|❌|
|/carrier-movement channel clear|❌|❌|✅|✅|
|/carrier-movement ignored-carriers add|❌|✅|✅|✅|
|/carrier-movement ignored-carriers export|❌|✅|✅|❌|
|/carrier-movement ignored-carriers import|❌|✅|✅|✅|
|/carrier-movement ignored-carriers list|✅|✅|✅|❌|
|/carrier-movement ignored-carriers remove|❌|✅|✅|✅|
|/todo-list show|✅|✅|✅|❌|
|/todo-list raw|❌|✅|✅|❌|
|/todo-list support set|❌|❌|✅|✅|✅|
|/todo-list support get|✅|✅|✅|❌|
|/todo-list support clear|❌|✅|✅|✅|
|/todo-list goal add|❌|✅|✅|✅|
|/todo-list goal export|❌|✅|✅|❌|
|/todo-list goal import|❌|✅|✅|✅|
|/todo-list goal list|✅|✅|✅|❌|
|/todo-list goal remove|❌|✅|✅|✅|
