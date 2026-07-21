USE [RWP_Sale_DB]
GO

/****** Object:  Table [dbo].[tErrorsOfLog]    Script Date: 11/5/2024 10:04:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[tErrorsOfLog](
	[fID] [bigint] IDENTITY(1,1) NOT NULL,
	[fLogUID] [uniqueidentifier] NOT NULL,
	[fErrorType] [bigint] NOT NULL,
	[fCaption] [nvarchar](max) NULL,
	[fErrorCode] [nvarchar](max) NULL,
	[fSendLogUID] [uniqueidentifier] NULL,
	[fInvoiceId] [nvarchar](100) NULL,
 CONSTRAINT [PK_tErrorsOfLog] PRIMARY KEY CLUSTERED 
(
	[fID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO


