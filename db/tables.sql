IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DiscordGuidSystemMinorFactionGoal]') AND type in (N'U'))
DROP TABLE [dbo].[DiscordGuidSystemMinorFactionGoal]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[State]') AND type in (N'U'))
DROP TABLE [dbo].[State]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StarSystemMinorFaction]') AND type in (N'U'))
DROP TABLE [dbo].[StarSystemMinorFaction]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DiscordGuild]') AND type in (N'U'))
DROP TABLE [dbo].[DiscordGuild]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StarSystem]') AND type in (N'U'))
DROP TABLE [dbo].[StarSystem]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MinorFaction]') AND type in (N'U'))
DROP TABLE [dbo].[MinorFaction]
GO

CREATE TABLE [dbo].[DiscordGuild](
	[ID] [int] IDENTITY(1,1) PRIMARY KEY,
	[Snowflake] [nvarchar](20) NOT NULL,
)

CREATE UNIQUE INDEX [IX_DiscordGuild_Snowflake] 
ON [dbo].[DiscordGuild]([Snowflake])

CREATE TABLE [dbo].[StarSystem](
	[ID] [int] IDENTITY(1,1) PRIMARY KEY,
	[Name] [nvarchar](100) NOT NULL,
	[LastUpdated] [datetime] NOT NULL
)

CREATE UNIQUE INDEX [IX_StarSystem_Name] 
ON [dbo].[StarSystem]([Name])

CREATE TABLE [dbo].[MinorFaction](
	[ID] [int] IDENTITY(1,1) PRIMARY KEY,
	[Name] [nvarchar](100) NOT NULL,
)

CREATE UNIQUE INDEX [IX_MinorFaction_Name] 
ON [dbo].[MinorFaction]([Name])

CREATE TABLE [dbo].[StarSystemMinorFaction](
	[ID] [int] IDENTITY(1,1) PRIMARY KEY,
	[StarSystemID] [int] FOREIGN KEY REFERENCES [StarSystem]([ID]) ON DELETE CASCADE,
	[MinorFactionID] [int] FOREIGN KEY REFERENCES [MinorFaction]([ID]) ON DELETE CASCADE,
	[Influence] [float] NULL,
)

CREATE UNIQUE INDEX [IX_SystemMinorFaction_SystemMinorFaction] 
ON [dbo].[StarSystemMinorFaction]([StarSystemID], [MinorFactionID])

CREATE TABLE [dbo].[State](
	[ID] [int] IDENTITY(1,1) PRIMARY KEY,
	[StarSystemMinorFactionID] [int] FOREIGN KEY REFERENCES [StarSystemMinorFaction]([ID]) ON DELETE CASCADE,
	[Name] [nvarchar](100) NOT NULL
)

CREATE INDEX [IX_SystemMinorFactionState_SystemMinorFaction] 
ON [dbo].[State]([SystemMinorFactionID])

CREATE UNIQUE INDEX [IX_SystemMinorFactionState_SystemMinorFactionState] 
ON [dbo].[State]([SystemMinorFactionID], [Name])

CREATE TABLE [dbo].[DiscordGuidSystemMinorFactionGoal](
	[ID] [int] IDENTITY(1,1) PRIMARY KEY,
	[DiscordGuidID] [int] FOREIGN KEY REFERENCES [DiscordGuild]([ID]) ON DELETE CASCADE,
	[StarSystemMinorFactionID] [int] FOREIGN KEY REFERENCES [StarSystemMinorFaction]([ID]) ON DELETE CASCADE,
	[Goal] [nvarchar](100) NOT NULL
)

CREATE INDEX [IX_DiscordGuidSystemMinorFactionGoal_SystemMinorFaction] 
ON [dbo].[DiscordGuidSystemMinorFactionGoal]([DiscordGuidID], [StarSystemMinorFactionID])

