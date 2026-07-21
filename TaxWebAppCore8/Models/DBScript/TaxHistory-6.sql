USE [RWP_Sale_DB]
GO

/****** Object:  Table [dbo].[TaxHistory]    Script Date: 11/5/2024 10:23:04 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TaxHistory](
	[InvoiceId] [nvarchar](100) NULL,
	[StoreName] [nvarchar](50) NULL,
	[CustomerName] [nvarchar](152) NULL,
	[InvoiceNumberSt] [nvarchar](50) NULL,
	[TotalPrice] [decimal](38, 0) NULL,
	[TotalDiscount] [decimal](38, 0) NULL,
	[TotalTax] [decimal](38, 0) NULL,
	[NetPrice] [decimal](38, 0) NULL,
	[BusinessDate] [date] NULL,
	[StoreID] [int] NULL,
	[fID] [bigint] IDENTITY(1,1) NOT NULL,
 CONSTRAINT [PK_TaxHistory] PRIMARY KEY CLUSTERED 
(
	[fID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO


