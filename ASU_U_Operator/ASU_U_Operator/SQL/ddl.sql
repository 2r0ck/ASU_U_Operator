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
	[Path] [varchar](100) NOT NULL,
	[LastInitDate] [datetime] NOT NULL,
	[ThrowIfInitError] [bit] NOT NULL,
	[ThrowIfHealthcheckError] [bit] NOT NULL,
	[ShutdownTimeoutMs] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[Key] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [operator].[Workers] ADD  DEFAULT ((0)) FOR [ThrowIfInitError]
GO
ALTER TABLE [operator].[Workers] ADD  DEFAULT ((0)) FOR [ThrowIfHealthcheckError]
GO
