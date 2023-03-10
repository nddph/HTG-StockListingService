USE [HTGStockDeal]
GO
/****** Object:  Table [dbo].[StockDealDetail]    Script Date: 2021-12-08 3:40:34 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StockDealDetail]') AND type in (N'U'))
DROP TABLE [dbo].[StockDealDetail]
GO
/****** Object:  Table [dbo].[StockDeal]    Script Date: 2021-12-08 3:40:34 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StockDeal]') AND type in (N'U'))
DROP TABLE [dbo].[StockDeal]
GO
/****** Object:  Table [dbo].[StockDeal]    Script Date: 2021-12-08 3:40:34 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StockDeal](
	[Id] [uniqueidentifier] NOT NULL,
	[TickeId] [uniqueidentifier] NULL,
	[SenderId] [uniqueidentifier] NOT NULL,
	[ReceiverId] [uniqueidentifier] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ReceiverName] [nvarchar](max) NULL,
	[SenderName] [nvarchar](max) NULL,
 CONSTRAINT [PK_StockDeal] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[StockDealDetail]    Script Date: 2021-12-08 3:40:34 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StockDealDetail](
	[Id] [uniqueidentifier] NOT NULL,
	[SenderName] [nvarchar](max) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[DeletedDate] [datetime] NULL,
	[ModifiedDate] [datetime] NULL,
	[StockDetailId] [uniqueidentifier] NOT NULL,
	[Quantity] [decimal](20, 0) NOT NULL,
	[UnitPrice] [decimal](18, 2) NOT NULL,
	[TotalPrice] [decimal](18, 2) NOT NULL,
 CONSTRAINT [PK_StockDealDetail] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
