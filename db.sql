CREATE DATABASE [OrderBot]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'OrderBot', FILENAME = N'/var/opt/mssql/data/OrderBot.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'OrderBot_log', FILENAME = N'/var/opt/mssql/data/OrderBot_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT
GO

IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [OrderBot].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO

USE [OrderBot]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SystemMinorFaction]') AND type in (N'U'))
DROP TABLE [dbo].[SystemMinorFaction]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SystemMinorFactionState]') AND type in (N'U'))
DROP TABLE [dbo].[SystemMinorFactionState]
GO

CREATE TABLE [dbo].[SystemMinorFaction](
	[ID] [int] IDENTITY(1,1),
	[System] [nvarchar](100) NOT NULL,
	[MinorFaction] [nvarchar](100) NOT NULL,
	[Influence] [float] NULL,
	[Goal] [nvarchar](100) NULL,
	[LastUpdated] [datetime] NOT NULL
 CONSTRAINT [PK_SystemMinorFaction] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[SystemMinorFactionState](
	[SystemMinorFactionId] [int] NOT NULL,
	[State] [nvarchar](100) NOT NULL,
 CONSTRAINT [PK_SystemMinorFactionState] PRIMARY KEY CLUSTERED 
(
	[SystemMinorFactionId] ASC,
	[State] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

USE [master]
GO

CREATE LOGIN [OrderBot] WITH PASSWORD=N'<password>', DEFAULT_DATABASE=[OrderBot], DEFAULT_LANGUAGE=[us_english], CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF
GO

USE [OrderBot]
GO

CREATE USER [OrderBot] FOR LOGIN [OrderBot] WITH DEFAULT_SCHEMA=[dbo]
GO



