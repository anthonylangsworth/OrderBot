-- Some useful queries for debugging or seeing how the bot is used.

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

-- Basic Report
select ss.Name as [Star System], mf.Name as [Minor Faction], ssmf.Influence
from StarSystem ss left join StarSystemMinorFaction ssmf
	on ss.Id = ssmf.StarSystemId
	left join MinorFaction mf
	on ssmf.MinorFactionId = mf.Id
order by ss.Name, ssmf.Influence desc

-- Ignored carriers
select dg.GuildId, c.SerialNumber, c.Name, ss.Name
from IgnoredCarrier ic left join DiscordGuild dg on ic.DiscordGuildId = dg.Id
	left join Carrier c on ic.CarrierId = c.Id
	left join StarSystem ss on c.StarSystemId = ss.Id

-- Conflicts
select ss.Name as [Star System], mf1.Name as [Minor Faction 1], c.MinorFaction1WonDays, mf2.Name as [Minor Faction 2], c.MinorFaction2WonDays, c.Status, c.WarType
from Conflict c left join StarSystem ss on c.StarSystemId = ss.Id
left join MinorFaction mf1 on c.MinorFaction1Id = mf1.Id
left join MinorFaction mf2 on c.MinorFaction2Id = mf2.Id

-- All Systems tracked for a Canonn
select mf.Name, ss.Name
from MinorFaction mf
left join Presence p on p.MinorFactionId = mf.Id
left join StarSystem ss on p.StarSystemId = ss.Id
where mf.Name = 'Canonn'

-- Minor faction support for each guild
select dg.Name as [Discord Guild], mf.Name as [Minor Faction]
from DiscordGuildMinorFaction dgmf
left outer join DiscordGuild dg on dgmf.DiscordGuildId = dg.Id
left outer join MinorFaction mf on dgmf.MinorFactionId = mf.Id

-- Goals by Guild
select dg.Name, dgpg.Goal, dg.Name, mf.Name
from DiscordGuildPresenceGoal dgpg
left outer join DiscordGuild dg on dgpg.DiscordGuildId = dg.Id
left outer join Presence p on dgpg.PresenceId = p.Id
left outer join MinorFaction mf on p.MinorFactionId = mf.Id
order by dg.Name, dgpg.Goal
