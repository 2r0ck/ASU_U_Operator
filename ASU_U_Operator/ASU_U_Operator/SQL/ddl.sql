CREATE SCHEMA [operator]
GO
/****** Object:  Table [operator].[Workers]    Script Date: 04.10.2019 17:39:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [operator].[Workers](
	[Key] [uniqueidentifier] NOT NULL,
	[Name] [varchar](100) NOT NULL,
	[Description] [varchar](200) NULL,
	[Version] [varchar](12) NOT NULL,
	[Path] [varchar](max) NOT NULL,
	[LastLoadDate] [datetime]  NULL, 
	[LastInitDate] [datetime]  NULL, 
	[Loaded] [BIT] not NULL,
	[Init] [BIT] not NULL,
PRIMARY KEY CLUSTERED 
(
	[Key] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
 
 CREATE SCHEMA [shell]
GO
/****** Object:  Table [operator].[Workers]    Script Date: 04.10.2019 17:39:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [shell].[Shedulers](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Command] [varchar](100) NOT NULL, 
	[JsonData] [varchar](max)  NULL,
	[ProcessingDate] [datetime]  NULL, 
	[Info] [varchar](max)  NULL,
	[Status] [TINYINT]  NULL, 
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO