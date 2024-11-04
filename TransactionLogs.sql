USE [2C2P]
GO

/****** Object:  Table [dbo].[TransactionLogs]    Script Date: 04-Nov-24 1:52:27 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TransactionLogs](
	[LogId] [int] IDENTITY(1,1) NOT NULL,
	[TransactionId] [varchar](50) NULL,
	[FileName] [varchar](255) NOT NULL,
	[LogMessage] [varchar](255) NOT NULL,
	[CreatedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[LogId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[TransactionLogs] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO

