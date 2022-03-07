IF  EXISTS (SELECT * FROM sys.objects WHERE object_Id = OBJECT_Id(N'[dbo].[DiscordGuidSystemMinorFactionGoal]') AND type in (N'U'))
DROP TABLE [dbo].[DiscordGuidSystemMinorFactionGoal]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_Id = OBJECT_Id(N'[dbo].[StarSystemMinorFactionState]') AND type in (N'U'))
DROP TABLE [dbo].[StarSystemMinorFactionState]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_Id = OBJECT_Id(N'[dbo].[StarSystemMinorFaction]') AND type in (N'U'))
DROP TABLE [dbo].[StarSystemMinorFaction]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_Id = OBJECT_Id(N'[dbo].[DiscordGuild]') AND type in (N'U'))
DROP TABLE [dbo].[DiscordGuild]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_Id = OBJECT_Id(N'[dbo].[StarSystem]') AND type in (N'U'))
DROP TABLE [dbo].[StarSystem]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_Id = OBJECT_Id(N'[dbo].[MinorFaction]') AND type in (N'U'))
DROP TABLE [dbo].[MinorFaction]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_Id = OBJECT_Id(N'[dbo].[State]') AND type in (N'U'))
DROP TABLE [dbo].[State]
GO

--CREATE TABLE [dbo].[DiscordGuild](
--	[Id] [int] IdENTITY(1,1) PRIMARY KEY,
--	[Snowflake] [nvarchar](20) NOT NULL,
--)

--CREATE UNIQUE INDEX [IX_DiscordGuild_Snowflake] 
--ON [dbo].[DiscordGuild]([Snowflake])

CREATE TABLE [dbo].[StarSystem](
	[Id] [int] IdENTITY(1,1) PRIMARY KEY,
	[Name] [nvarchar](100) NOT NULL,
	[LastUpdated] [datetime] NOT NULL
)

CREATE UNIQUE INDEX [IX_StarSystem_Name] 
ON [dbo].[StarSystem]([Name])

CREATE TABLE [dbo].[MinorFaction](
	[Id] [int] IDENTITY(1,1) PRIMARY KEY,
	[Name] [nvarchar](100) NOT NULL,
)

CREATE UNIQUE INDEX [IX_MinorFaction_Name] 
ON [dbo].[MinorFaction]([Name])

CREATE TABLE [dbo].[State](
	[Id] [int] IDENTITY(1,1) PRIMARY KEY,
	[Name] [nvarchar](100) NOT NULL,
)

CREATE UNIQUE INDEX [IX_State] 
ON [dbo].[StarSystem]([Name])

CREATE TABLE [dbo].[StarSystemMinorFaction](
	[Id] [int] IDENTITY(1,1) PRIMARY KEY,
	[StarSystemId] [int] NOT NULL FOREIGN KEY REFERENCES [StarSystem]([Id]) ON DELETE CASCADE,
	[MinorFactionId] [int] NOT NULL FOREIGN KEY REFERENCES [MinorFaction]([Id]) ON DELETE CASCADE,
	[Influence] [float] NULL,
)

CREATE UNIQUE INDEX [IX_StarSystemMinorFaction_SystemMinorFaction] 
ON [dbo].[StarSystemMinorFaction]([StarSystemId], [MinorFactionId])

-- Column name plurals required for EF Core name inference. Will fix later.
CREATE TABLE [dbo].[StarSystemMinorFactionState](
	[StarSystemMinorFactionsId] [int] NOT NULL FOREIGN KEY REFERENCES [StarSystemMinorFaction]([Id]) ON DELETE CASCADE,
	[StatesId] [int] NOT NULL FOREIGN KEY REFERENCES [State]([Id]) ON DELETE CASCADE,
	PRIMARY KEY ([StarSystemMinorFactionsId], [StatesId])
)

CREATE INDEX [IX_StarSystemMinorFactionState_SystemMinorFaction] 
ON [dbo].[StarSystemMinorFactionState]([StarSystemMinorFactionsId])

--CREATE TABLE [dbo].[DiscordGuidSystemMinorFactionGoal](
--	[Id] [int] IDENTITY(1,1) PRIMARY KEY,
--	[DiscordGuildId] [int] FOREIGN KEY REFERENCES [DiscordGuild]([Id]) ON DELETE CASCADE,
--	[StarSystemMinorFactionId] [int] FOREIGN KEY REFERENCES [StarSystemMinorFaction]([Id]) ON DELETE CASCADE,
--	[Goal] [nvarchar](100) NOT NULL
--)

--CREATE INDEX [IX_DiscordGuIdSystemMinorFactionGoal_SystemMinorFaction] 
--ON [dbo].[DiscordGuIdSystemMinorFactionGoal]([DiscordGuildId], [StarSystemMinorFactionId])

