USE [RWP_Sale_DB]
GO

/****** Object:  Table [dbo].[tTisTaxErrorType]    Script Date: 11/5/2024 10:15:01 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[tTisTaxErrorType](
	[fID] [bigint] IDENTITY(1,1) NOT NULL,
	[fType] [smallint] NOT NULL,
	[fCaption] [nvarchar](max) NOT NULL,
	[fDescription] [nvarchar](max) NOT NULL,
	[fCreatedDateTime] [datetime] NOT NULL,
	[fCode] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_tTisTaxErrorType] PRIMARY KEY CLUSTERED 
(
	[fID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[tTisTaxErrorType] ADD  CONSTRAINT [DF_tTisTaxErrorType_fType]  DEFAULT ((0)) FOR [fType]
GO

ALTER TABLE [dbo].[tTisTaxErrorType] ADD  CONSTRAINT [DF_tTisTaxErrorType_fCreatedDateTime]  DEFAULT (sysdatetime()) FOR [fCreatedDateTime]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'نوع مربوط به تیس یا تکس (0 تکس و یک تیس)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'tTisTaxErrorType', @level2type=N'COLUMN',@level2name=N'fType'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'جد.ل ذخیره سازی انواع خطا در تیس و تکس' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'tTisTaxErrorType'
GO


