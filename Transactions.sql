USE [2C2P]
GO

/****** Object:  Table [dbo].[Transactions]    Script Date: 04-Nov-24 1:51:33 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Transactions](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TransactionId] [varchar](50) NOT NULL,
	[AccountNumber] [varchar](30) NULL,
	[Amount] [decimal](18, 2) NULL,
	[CurrencyCode] [char](3) NULL,
	[TransactionDate] [datetime] NULL,
	[Status] [nvarchar](50) NULL,
	[CreatedAt] [datetime] NULL,
 CONSTRAINT [PK__Transact__3214EC07E53C86E8] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Transactions] ADD  CONSTRAINT [DF__Transacti__Creat__25869641]  DEFAULT (getdate()) FOR [CreatedAt]
GO

