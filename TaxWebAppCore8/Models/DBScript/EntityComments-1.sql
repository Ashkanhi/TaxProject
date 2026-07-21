USE [RWP_Sale_DB]
GO

/****** Object:  Table [dbo].[tEntityComments]    Script Date: 11/5/2024 9:51:02 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[tEntityComments](
	[fID] [bigint] IDENTITY(1,1) NOT NULL,
	[fComment] [nvarchar](max) NOT NULL,
	[fPersonUniqueID] [uniqueidentifier] NOT NULL,
	[fEntityUniqueID] [uniqueidentifier] NOT NULL,
	[fDeleted] [bit] NOT NULL,
	[fCreatedDate] [datetime] NOT NULL,
	[fCommentUniqueID] [uniqueidentifier] NOT NULL,
	[fSubject] [nvarchar](max) NULL,
	[fConvertID] [bigint] NULL,
	[fEventDate] [datetime] NULL,
	[fIsSelfComment] [bit] NOT NULL,
	[fIP] [nvarchar](max) NULL,
 CONSTRAINT [PK_tEntityComments] PRIMARY KEY CLUSTERED 
(
	[fID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[tEntityComments] ADD  CONSTRAINT [DF_tEntityComments_fIsSelfComment]  DEFAULT ((0)) FOR [fIsSelfComment]
GO


