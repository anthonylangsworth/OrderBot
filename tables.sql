IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DiscordGuidSystemMinorFactionGoal]') AND type in (N'U'))
DROP TABLE [dbo].[DiscordGuidSystemMinorFactionGoal]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SystemMinorFactionState]') AND type in (N'U'))
DROP TABLE [dbo].[SystemMinorFactionState]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SystemMinorFaction]') AND type in (N'U'))
DROP TABLE [dbo].[SystemMinorFaction]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DiscordGuild]') AND type in (N'U'))
DROP TABLE [dbo].[DiscordGuild]
GO
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[System]') AND type in (N'U'))
--DROP TABLE [dbo].[System]
--GO
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MinorFaction]') AND type in (N'U'))
--DROP TABLE [dbo].[MinorFaction]
--GO

CREATE TABLE [dbo].[DiscordGuild](
	[ID] [int] IDENTITY(1,1) PRIMARY KEY,
	[Snowflake] [nvarchar](20) NOT NULL,
)

CREATE UNIQUE INDEX [IX_DiscordGuild_Snowflake] 
ON [dbo].[DiscordGuild]([Snowflake])

--CREATE TABLE [dbo].[System](
--	[ID] [int] IDENTITY(1,1) PRIMARY KEY,
--	[Name] [nvarchar](100) NOT NULL,
--)

--CREATE UNIQUE INDEX [IX_System_Name] 
--ON [dbo].[System]([Name])

--CREATE TABLE [dbo].[MinorFaction](
--	[ID] [int] IDENTITY(1,1) PRIMARY KEY,
--	[Name] [nvarchar](100) NOT NULL,
--)

--CREATE UNIQUE INDEX [IX_MinorFaction_Name] 
--ON [dbo].[MinorFaction]([Name])

CREATE TABLE [dbo].[SystemMinorFaction](
	[ID] [int] IDENTITY(1,1) PRIMARY KEY,
	[System] [nvarchar](100),
	[MinorFaction] [nvarchar](100),
	[Influence] [float] NULL,
	[LastUpdated] [datetime] NOT NULL
)

CREATE UNIQUE INDEX [IX_SystemMinorFaction_SystemMinorFaction] 
ON [dbo].[SystemMinorFaction]([System], [MinorFaction])

CREATE TABLE [dbo].[SystemMinorFactionState](
	[ID] [int] IDENTITY(1,1) PRIMARY KEY,
	[SystemMinorFactionID] [int] FOREIGN KEY REFERENCES [SystemMinorFaction]([ID]),
	[State] [nvarchar](100) NOT NULL
)

CREATE INDEX [IX_SystemMinorFactionState_SystemMinorFaction] 
ON [dbo].[SystemMinorFactionState]([SystemMinorFactionID])

CREATE TABLE [dbo].[DiscordGuidSystemMinorFactionGoal](
	[ID] [int] IDENTITY(1,1) PRIMARY KEY,
	[DiscordGuidID] [int] FOREIGN KEY REFERENCES [DiscordGuild]([ID]),
	[SystemMinorFactionID] [int] FOREIGN KEY REFERENCES [SystemMinorFaction]([ID]),
	[Goal] [nvarchar](100) NOT NULL
)

CREATE INDEX [IX_DiscordGuidSystemMinorFactionGoal_SystemMinorFaction] 
ON [dbo].[DiscordGuidSystemMinorFactionGoal]([SystemMinorFactionID])

