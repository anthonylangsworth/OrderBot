IF  EXISTS (SELECT * FROM sys.objects WHERE object_Id = OBJECT_Id(N'[dbo].[IgnoredCarrier]') AND type in (N'U'))
DROP TABLE [dbo].[IgnoredCarrier]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_Id = OBJECT_Id(N'[dbo].[StarSystemCarrier]') AND type in (N'U'))
DROP TABLE [dbo].[StarSystemCarrier]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_Id = OBJECT_Id(N'[dbo].[Carrier]') AND type in (N'U'))
DROP TABLE [dbo].[Carrier]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_Id = OBJECT_Id(N'[dbo].[DiscordGuildStarSystemMinorFactionGoal]') AND type in (N'U'))
DROP TABLE [dbo].[DiscordGuildStarSystemMinorFactionGoal]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_Id = OBJECT_Id(N'[dbo].[PresenceState]') AND type in (N'U'))
DROP TABLE [dbo].[PresenceState]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_Id = OBJECT_Id(N'[dbo].[Presence]') AND type in (N'U'))
DROP TABLE [dbo].Presence
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_Id = OBJECT_Id(N'[dbo].[DiscordGuildMinorFaction]') AND type in (N'U'))
DROP TABLE [dbo].[DiscordGuildMinorFaction]
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


CREATE TABLE [dbo].[DiscordGuild](
	[Id] [int] IDENTITY(1,1) PRIMARY KEY,
	[GuildId] [numeric](20,0) NOT NULL,
	[Name] [nvarchar](100) NULL,
	[CarrierMovementChannel] [numeric](20,0) NULL 
)
GO
CREATE UNIQUE INDEX [IX_DiscordGuild_GuildId] 
ON [dbo].[DiscordGuild]([GuildId])
GO
CREATE TABLE [dbo].[DiscordGuildMinorFaction](
	[Id] [int] IDENTITY(1,1) PRIMARY KEY,
	[DiscordGuildId] [int] NOT NULL,
	[MinorFactionId] [int] NOT NULL,
)
GO
CREATE UNIQUE INDEX [IX_DiscordGuild_GuildMinorFaction]
ON [dbo].[DiscordGuildMinorFaction]([DiscordGuildId], [MinorFactionId])
GO
CREATE TABLE [dbo].[StarSystem](
	[Id] [int] IdENTITY(1,1) PRIMARY KEY,
	[Name] [nvarchar](100) NOT NULL,
	[LastUpdated] [datetime] NULL
)
GO
CREATE UNIQUE INDEX [IX_StarSystem_Name] 
ON [dbo].[StarSystem]([Name])
GO
CREATE TABLE [dbo].[MinorFaction](
	[Id] [int] IDENTITY(1,1) PRIMARY KEY,
	[Name] [nvarchar](100) NOT NULL,
)
GO
CREATE UNIQUE INDEX [IX_MinorFaction_Name] 
ON [dbo].[MinorFaction]([Name])
GO
CREATE TABLE [dbo].[State](
	[Id] [int] IDENTITY(1,1) PRIMARY KEY,
	[Name] [nvarchar](100) NOT NULL,
)
GO
CREATE UNIQUE INDEX [IX_State] 
ON [dbo].[State]([Name])
GO
CREATE TABLE [dbo].[Presence](
	[Id] [int] IDENTITY(1,1) PRIMARY KEY,
	[StarSystemId] [int] NOT NULL FOREIGN KEY REFERENCES [StarSystem]([Id]) ON DELETE CASCADE,
	[MinorFactionId] [int] NOT NULL FOREIGN KEY REFERENCES [MinorFaction]([Id]) ON DELETE CASCADE,
	[Influence] [float] NULL,
	[SecurityLevel] [nvarchar](100) NULL
)
GO
CREATE UNIQUE INDEX [IX_Presence_SystemMinorFaction] 
ON [dbo].[Presence]([StarSystemId], [MinorFactionId])
GO
-- Column name plurals required for EF Core name inference. Will fix later.
CREATE TABLE [dbo].[PresenceState](
	[PresenceId] [int] NOT NULL FOREIGN KEY REFERENCES [Presence]([Id]) ON DELETE CASCADE,
	[StatesId] [int] NOT NULL FOREIGN KEY REFERENCES [State]([Id]) ON DELETE CASCADE,
	PRIMARY KEY ([PresenceId], [StatesId])
)
GO
CREATE INDEX [IX_StarSystemMinorFactionState_SystemMinorFaction] 
ON [dbo].[StarSystemMinorFactionState]([StarSystemMinorFactionsId])
GO
CREATE TABLE [dbo].[DiscordGuildPresenceGoal](
	[Id] [int] IDENTITY(1,1) PRIMARY KEY,
	[DiscordGuildId] [int] NOT NULL FOREIGN KEY REFERENCES [DiscordGuild]([Id]) ON DELETE CASCADE,
	[PresenceId] [int] NOT NULL FOREIGN KEY REFERENCES [Presence]([Id]) ON DELETE CASCADE,
	[Goal] [nvarchar](100) NOT NULL
)
GO
create unique INDEX [IX_DiscordGuildPresenceGoal_DiscordGuildPresence] 
ON [dbo].[DiscordGuildPresenceGoal]([DiscordGuildId], [PresenceId])
GO
CREATE TABLE [dbo].[Carrier](
	[Id] [int] IDENTITY(1,1) PRIMARY KEY,
	[SerialNumber] [char](7) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Owner] [nvarchar](100) NULL,
	[StarSystemId] [int] NULL FOREIGN KEY REFERENCES [StarSystem]([Id]) ON DELETE CASCADE,
	[FirstSeen] [datetime] NULL
)
GO
CREATE UNIQUE INDEX [IX_Carrier_Name] 
ON [dbo].[Carrier]([Name])
GO
CREATE UNIQUE INDEX [IX_Carrier_SerialNumber]
ON [dbo].[Carrier]([SerialNumber])
GO
CREATE TABLE [dbo].[IgnoredCarrier](
	[Id] [int] IDENTITY(1,1) PRIMARY KEY,
	[CarrierId] [int] NOT NULL FOREIGN KEY REFERENCES [Carrier]([Id]) ON DELETE CASCADE,
	[DiscordGuildId] [int] NOT NULL FOREIGN KEY REFERENCES [DiscordGuild]([Id]) ON DELETE CASCADE
)
GO
CREATE UNIQUE INDEX [IX_IgnoredCarrier_CarrerDiscordGuid]
ON [dbo].[IgnoredCarrier]([CarrierId], [DiscordGuildId])
GO
CREATE TABLE [dbo].[Conflict](
	[Id] [int] IDENTITY(1,1) PRIMARY KEY,
	[StarSystemId] [int] NOT NULL FOREIGN KEY REFERENCES [StarSystem]([Id]) ON DELETE CASCADE,
	[MinorFaction1Id] [int] NOT NULL FOREIGN KEY REFERENCES [MinorFaction]([Id]),
	[MinorFaction1WonDays] [int] NOT NULL,
	[MinorFaction2Id] [int] NOT NULL FOREIGN KEY REFERENCES [MinorFaction]([Id]),
	[MinorFaction2WonDays] [int] NOT NULL,
	[Status] [nvarchar](100) NULL,
	[WarType] [nvarchar](100) NOT NULL
)
GO
CREATE UNIQUE INDEX [IX_Conflict_StarSystemFactions]
ON [dbo].[Conflict]([StarSystemId], [MinorFaction1Id], [MinorFaction2Id])
GO
CREATE INDEX [IX_Conflict_StarSystem]
ON [dbo].[Conflict]([StarSystemId])
GO
