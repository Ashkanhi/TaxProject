USE [RWP_Sale_DB]
GO

/****** Object:  Table [dbo].[tTaxHistoryStateParallel]    Script Date: 11/5/2024 10:21:51 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[tTaxHistoryStateParallel](
	[fID] [bigint] IDENTITY(1,1) NOT NULL,
	[fInvoiceID] [nvarchar](max) NOT NULL,
	[fSendState] [int] NOT NULL,
	[fListOfErrorCode] [nvarchar](max) NOT NULL,
	[fListOfErrorCaption] [nvarchar](max) NOT NULL,
	[ReferenceNumber] [nvarchar](max) NULL,
	[fLastSendDate] [datetime] NULL,
 CONSTRAINT [PK_tTaxHistoryStateParallel] PRIMARY KEY CLUSTERED 
(
	[fID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO


