USE [RWP_Sale_DB]
GO

/****** Object:  Table [dbo].[tSendInvoiceTaxTissLog]    Script Date: 11/5/2024 9:54:18 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[tSendInvoiceTaxTissLog](
	[fID] [bigint] IDENTITY(1,1) NOT NULL,
	[fReferencedID] [nvarchar](max) NOT NULL,
	[fInvoiceID] [nvarchar](max) NOT NULL,
	[fLastSendDate] [datetime] NOT NULL,
	[fDeleted] [bit] NOT NULL,
	[fOrder] [smallint] NOT NULL,
	[fStatus] [smallint] NOT NULL,
	[fLogUID] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_tSendInvoiceTaxTissLog] PRIMARY KEY CLUSTERED 
(
	[fID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO


