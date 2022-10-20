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