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
