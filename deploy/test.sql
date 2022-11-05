-- Influence for each minor faction in each system
select ss.Name as [Star System], mf.Name as [Minor Faction], ssmf.Influence
from StarSystemMinorFaction ssmf left join StarSystem ss
	on ssmf.StarSystemId = ss.Id
left join MinorFaction mf
	on ssmf.MinorFactionId = mf.Id
go

-- System state
select ss.Name as [Star System], mf.Name as [Minor Faction], State.Name as [State]
from StarSystemMinorFactionState ssmfs left join StarSystemMinorFaction ssmf
	on ssmfs.StarSystemMinorFactionsId = ssmf.Id
left join StarSystem ss
	on ssmf.StarSystemId = ss.Id
left join MinorFaction mf
	on ssmf.MinorFactionId = mf.Id
left join State 
	on ssmfs.StatesId = State.Id
go

/* Basic Report */
select ss.Name as [Star System], mf.Name as [Minor Faction], ssmf.Influence
from StarSystem ss left join StarSystemMinorFaction ssmf
	on ss.Id = ssmf.StarSystemId
	left join MinorFaction mf
	on ssmf.MinorFactionId = mf.Id
order by ss.Name, ssmf.Influence desc

/*
select * from MinorFaction
select * from StarSystem
select * from State
*/

/*
delete from State
delete from StarSystem
delete from MinorFaction
delete from StarSystemMinorFaction
delete from StarSystemMinorFactionState
*/

-- Ignored carriers
select dg.GuildId, c.SerialNumber, c.Name
from IgnoredCarrier ic left join DiscordGuild dg on ic.DiscordGuildId = dg.Id
	left join Carrier c on ic.CarrierId = c.Id

-- Conflicts
select ss.Name as [Star System], mf1.Name as [Minor Faction 1], c.MinorFaction1WonDays, mf2.Name as [Minor Faction 2], c.MinorFaction2WonDays, c.Status, c.WarType
from Conflict c left join StarSystem ss on c.StarSystemId = ss.Id
left join MinorFaction mf1 on c.MinorFaction1Id = mf1.Id
left join MinorFaction mf2 on c.MinorFaction2Id = mf2.Id

/*
-- To convert on 5 Nov 2022
exec sp_rename 'dnbpStarSystemMinorFaction', 'Presence'
go
exec sp_rename 'StarSystemMinorFaction', 'Presence'
go
drop index [IX_StarSystemMinorFaction_SystemMinorFaction] 
on [dbo].[Presence]
go
CREATE UNIQUE INDEX [IX_Presence_SystemMinorFaction] 
ON [dbo].[Presence]([StarSystemId], [MinorFactionId])
go
exec sp_rename 'StarSystemMinorFactionState', 'PresenceState'
go
EXEC sp_rename 'dbo.PresenceState.StarSystemMinorFactionsId', 'PresenceId', 'COLUMN';
go
drop index CREATE INDEX [IX_StarSystemMinorFactionState_SystemMinorFaction] 
ON [dbo].[StarSystemMinorFactionState]
go
CREATE INDEX [IX_PresenceState_SystemMinorFaction] 
ON [dbo].[PresenceState]([PresenceId])
GO
exec sp_rename 'DiscordGuildStarSystemMinorFactionGoal', 'DiscordGuildPresenceGoal'
go
exec sp_rename 'dbo.DiscordGuildPresenceGoal.StarSystemMinorFactionId', 'PresenceId'
go
drop INDEX [IX_DiscordGuildPresenceGoal_StarSystemMinorFaction] 
ON [dbo].[Presence]
GO
drop INDEX [IX_DiscordGuildStarSystemMinorFactionGoal_StarSystemMinorFaction] 
ON [dbo].[DiscordGuildPresenceGoal]
GO
create unique INDEX [IX_DiscordGuildPresenceGoal_DiscordGuildPresence] 
ON [dbo].[DiscordGuildPresenceGoal]([DiscordGuildId], [PresenceId])
GO
exec sp_rename 'dbo.Presence.Security', 'SecurityLevel'
go
*/