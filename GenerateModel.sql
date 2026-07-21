IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[TaxGovermentInoiceDetailLog]') AND type in (N'U'))
BEGIN
CREATE TABLE [TaxGovermentInoiceDetailLog](
	[Sstid] [nvarchar](255) NULL,
	[Sstt] [nvarchar](255) NULL,
	[Mu] [varchar](50) NULL,
	[Am] [decimal](19, 3) NULL,
	[Fee] [decimal](18, 0) NULL,
	[Prdis] [decimal](38, 6) NULL,
	[Dis] [decimal](20, 3) NULL,
	[Adis] [decimal](38, 6) NULL,
	[Vra] [decimal](18, 0) NULL,
	[Vam] [decimal](18, 0) NULL,
	[Odt] [varchar](50) NULL,
	[Odr] [decimal](18, 0) NULL,
	[Odam] [decimal](18, 0) NULL,
	[Olt] [varchar](50) NULL,
	[Olr] [decimal](18, 0) NULL,
	[Olam] [decimal](18, 0) NULL,
	[Consfee] [decimal](18, 0) NULL,
	[Spro] [decimal](18, 0) NULL,
	[Bros] [decimal](18, 0) NULL,
	[Tcpbs] [decimal](18, 0) NULL,
	[Cop] [decimal](38, 6) NULL,
	[Vop] [decimal](18, 0) NULL,
	[Bsrn] [varchar](1) NOT NULL,
	[Tsstam] [decimal](38, 6) NULL,
	[Nw] [decimal](18, 0) NULL,
	[Ssrv] [decimal](18, 0) NULL,
	[Sscv] [decimal](18, 0) NULL,
	[InvoiceId] [uniqueidentifier] NULL,
	[Date] [datetime] NULL
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[TaxGovermentInoiceHeaderLog]') AND type in (N'U'))
BEGIN
CREATE TABLE [TaxGovermentInoiceHeaderLog](
	[InvoiceId] [uniqueidentifier] NOT NULL,
	[CreateDate] [datetime] NULL,
	[SaveDate] [datetime] NULL,
	[Inty] [int] NOT NULL,
	[Taxid] [nvarchar](50) NULL,
	[Inno] [nvarchar](50) NULL,
	[Inp] [int] NOT NULL,
	[Ins] [int] NULL,
	[Tins] [varchar](50) NOT NULL,
	[Tob] [int] NOT NULL,
	[Bid] [varchar](50) NOT NULL,
	[Tinb] [varchar](50) NOT NULL,
	[Sbc] [varchar](50) NOT NULL,
	[Bpc] [varchar](50) NOT NULL,
	[Bbc] [varchar](50) NOT NULL,
	[Ft] [int] NOT NULL,
	[Scln] [varchar](50) NOT NULL,
	[Scc] [varchar](50) NOT NULL,
	[Crn] [varchar](50) NOT NULL,
	[Billid] [varchar](50) NOT NULL,
	[Tprdis] [decimal](18, 0) NULL,
	[Tdis] [decimal](18, 0) NULL,
	[Tadis] [decimal](18, 0) NULL,
	[Tvam] [decimal](18, 0) NULL,
	[Todam] [decimal](18, 0) NULL,
	[Tbill] [decimal](18, 0) NULL,
	[Setm] [int] NOT NULL,
	[Cap] [decimal](18, 0) NULL,
	[Insp] [decimal](18, 0) NULL,
	[Tvop] [decimal](18, 0) NULL,
	[Tax17] [decimal](18, 0) NULL,
	[Cdcn] [varchar](50) NOT NULL,
	[Cdcd] [int] NOT NULL,
	[Tonw] [decimal](18, 0) NULL,
	[Torv] [decimal](18, 0) NULL,
	[Tocv] [decimal](18, 0) NULL,
	[Date] [datetime] NULL
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[TaxGovermentInvoiceFinal]') AND type in (N'U'))
BEGIN
CREATE TABLE [TaxGovermentInvoiceFinal](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[InvoiceId] [nvarchar](50) NOT NULL,
	[LastStatus] [nvarchar](50) NULL,
	[Done] [bit] NULL,
	[TaxId] [nvarchar](50) NULL,
	[LastRefrenceId] [nvarchar](50) NULL,
	[LastResponseCheckTime] [datetime] NULL,
 CONSTRAINT [PK_TaxGovermentInvoiceFinal] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[TaxGovermentInvoiceInfo]') AND type in (N'U'))
BEGIN
CREATE TABLE [TaxGovermentInvoiceInfo](
	[TaxGovermentInvoiceInfoId] [bigint] IDENTITY(1,1) NOT NULL,
	[InvoiceId] [uniqueidentifier] NOT NULL,
	[ReferenceNumber] [nvarchar](100) NULL,
	[Status] [nvarchar](50) NULL,
	[InvoiceUniqueId] [nvarchar](50) NULL,
	[UId] [nvarchar](100) NULL,
	[SendDate] [datetime] NULL,
	[IsReturn] [bit] NULL,
	[Done] [bit] NULL,
 CONSTRAINT [PK_TaxGovermentInvoiceInfo] PRIMARY KEY CLUSTERED 
(
	[TaxGovermentInvoiceInfoId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[TaxGovermentSendLog]') AND type in (N'U'))
BEGIN
CREATE TABLE [TaxGovermentSendLog](
	[LogId] [bigint] IDENTITY(1,1) NOT NULL,
	[Date] [datetime] NULL,
	[Status] [nvarchar](50) NULL,
	[RefrenceId] [nvarchar](100) NULL,
	[UId] [nvarchar](100) NULL,
	[Json] [nvarchar](max) NULL,
	[TaxGovermentInvoiceInfoId] [nchar](10) NULL,
	[PacketType] [nvarchar](50) NULL,
 CONSTRAINT [PK_TaxGovermentSendLog] PRIMARY KEY CLUSTERED 
(
	[LogId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[TaxGovermentSettingInfo]') AND type in (N'U'))
BEGIN
CREATE TABLE [TaxGovermentSettingInfo](
	[StoreId] [int] NOT NULL,
	[PrivateKey] [nvarchar](max) NOT NULL,
	[ClientId] [nvarchar](50) NULL,
	[PrivateKeyPath] [nvarchar](500) NULL,
	[ResponseCheckerTimeInSec] [int] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[DF_TaxGovermentInoiceDetailLog_Date]') AND type = 'D')
BEGIN
ALTER TABLE [TaxGovermentInoiceDetailLog] ADD  CONSTRAINT [DF_TaxGovermentInoiceDetailLog_Date]  DEFAULT (getdate()) FOR [Date]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[DF_TaxGovermentInoiceHeaderLog_Date]') AND type = 'D')
BEGIN
ALTER TABLE [TaxGovermentInoiceHeaderLog] ADD  CONSTRAINT [DF_TaxGovermentInoiceHeaderLog_Date]  DEFAULT (getdate()) FOR [Date]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[DF__TaxGoverm__LastR__222DD727]') AND type = 'D')
BEGIN
ALTER TABLE [TaxGovermentInvoiceFinal] ADD  DEFAULT (getdate()) FOR [LastResponseCheckTime]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[DF_TaxGovermentInvoiceInfo_SendDate]') AND type = 'D')
BEGIN
ALTER TABLE [TaxGovermentInvoiceInfo] ADD  CONSTRAINT [DF_TaxGovermentInvoiceInfo_SendDate]  DEFAULT (getdate()) FOR [SendDate]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[TaxGovermentGetInvoiceDetailForSend]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [TaxGovermentGetInvoiceDetailForSend] AS' 
END
GO

ALTER PROCEDURE [TaxGovermentGetInvoiceDetailForSend]
(
    @InvoiceID NVARCHAR(50) = '',
    @IsReturn BIT = 0
)
AS
BEGIN

    DECLARE @Fee DECIMAL(18, 0),
            @DiscountP INT,
            @PriceAfterDiscount DECIMAL(18, 0),
            @Qty DECIMAL(18, 3);

    SELECT *
    INTO #tmpItems
    FROM dbo.SaleInvoiceLineItem
    WHERE InvoiceID = @InvoiceID;

    SELECT sili.InvoiceID,
           sili.LineItemID,
           DiscountPercent = ((sili.DiscountAmount + sili.ManualDiscount + sili.InvoiceDiscount)
                              / (PriceAmount * (Quantity + BonusQuantity))
                             ) * 100,
           Qty = (Quantity + BonusQuantity),
           Fee = (CASE
                      WHEN IsPriceWithTax = 1 THEN
                          PriceAmount - (PriceAmount * 0.09)
                      ELSE
                          PriceAmount
                  END
                 ),
           FullPrice = ((CASE
                             WHEN IsPriceWithTax = 1 THEN
                                 PriceAmount - (PriceAmount * 0.09)
                             ELSE
                                 PriceAmount
                         END
                        )
                       ) * ((Quantity + BonusQuantity))
    INTO #tmpCalcItems
    FROM dbo.SaleInvoiceLineItem sili
    WHERE InvoiceID = @InvoiceID;




    SELECT  Sstid = ICF.TaxBarcode,
           Sstt = ifmv.Name,
           Mu = (CASE
                     WHEN ifmv.UnitOfMeasureID = 248 THEN
                         N'1627'
                     ELSE
                         '1627'
                 END
                ),
           Am = CI.Qty,                                                                      -- تعداد
           Fee = CONVERT(DECIMAL(18, 0), CI.Fee),                                            --- Fee,
           Prdis = CONVERT(DECIMAL(18, 0), CI.FullPrice),                                    --مبلغ قبل تخفیف
           Dis = CONVERT(DECIMAL(18, 0), ((CI.FullPrice * CI.DiscountPercent) / 100)),       --مبلغ تخفیف
           Adis = CONVERT(DECIMAL(18, 0), (CI.FullPrice) - ((CI.FullPrice * CI.DiscountPercent) / 100)), --مبلغ بعد از تخفیف
           Vra = CONVERT(   DECIMAL(18, 2),
                            (CASE
                                 WHEN sili.IsPriceWithTax = 1 THEN
                                     9--0.09
                                 ELSE
                                     0
                             END
                            )
                        ),                                                                   --نرخ مالیات بر ارزش افزوده
           Vam = 
		   CONVERT(   DECIMAL(18, 0),
                            (CASE
                                 WHEN sili.IsPriceWithTax = 1 THEN
                            (CI.FullPrice - ((CI.FullPrice * CI.DiscountPercent) / 100)) * 0.09
                                 ELSE
                                     0
                             END
                            )
                        ),                                                                   --مبلغ مالیات بر ارزش افزوده                                                --sili.TaxAmount + sili.TollAmount,
                                                                                             --Tsstam = CASE
                                                                                             --             WHEN sili.IsPriceWithTax = 1 THEN
                                                                                             --((sili.Quantity + sili.BonusQuantity) * sili.PriceAmount) - sili.DiscountAmount - sili.InvoiceDiscount
                                                                                             --- sili.ManualDiscount - sili.RoundAmount
                                                                                             --             ELSE
                                                                                             --((sili.Quantity + sili.BonusQuantity) * sili.PriceAmount) - sili.DiscountAmount - sili.InvoiceDiscount
                                                                                             --- sili.ManualDiscount - sili.RoundAmount --+ sili.TaxAmount + sili.TollAmount 
                                                                                             --END,
           Odt = '',
           Odr = CONVERT(DECIMAL, 0),
           Odam = CONVERT(DECIMAL, 0),
           Olt = '',
           Olr = CONVERT(DECIMAL, 0),
           Olam = CONVERT(DECIMAL, 0),
           Consfee = CONVERT(DECIMAL, 0),
           Spro = CONVERT(DECIMAL, 0),
           Bros = CONVERT(DECIMAL, 0),
           Tcpbs = CONVERT(DECIMAL, 0),
           Cop = CONVERT(DECIMAL, 0),
                                                                                             --CONVERT(   DECIMAL(18, 0),
                                                                                             --                       (CASE
                                                                                             --                            WHEN sili.IsPriceWithTax = 1 THEN
                                                                                             --                       (CI.FullPrice - ((CI.FullPrice * CI.DiscountPercent) / 100)) * 0.09
                                                                                             --                            ELSE
                                                                                             --                                0
                                                                                             --                        END
                                                                                             --                       )
                                                                                             --                   ),
                                                                                             --CASE
                                                                                             --               WHEN sili.IsPriceWithTax = 1 THEN
                                                                                             --     ((sili.Quantity + sili.BonusQuantity) * sili.PriceAmount) - sili.TaxAmount - sili.TollAmount
                                                                                             --     - sili.DiscountAmount - sili.InvoiceDiscount - sili.ManualDiscount - sili.RoundAmount
                                                                                             --               ELSE
                                                                                             --     ((sili.Quantity + sili.BonusQuantity) * sili.PriceAmount) - sili.DiscountAmount - sili.InvoiceDiscount
                                                                                             --     - sili.ManualDiscount - sili.RoundAmount
                                                                                             --           END + (sili.TaxAmount + sili.TollAmount),                                                       --سهم نقدی از پرداخت
           Vop = CONVERT(DECIMAL(18, 0),0),--CONVERT(DECIMAL, 0),
           --CONVERT(   DECIMAL(18, 0),
           --           (CASE
           --                WHEN sili.IsPriceWithTax = 1 THEN
           --           (CI.FullPrice - ((CI.FullPrice * CI.DiscountPercent) / 100)) * 0.09
           --                ELSE
           --                    0
           --            END
           --           )
           --       ),                                                                         --سهم ارزش افزوده از پرداخت                                                --sili.TaxAmount + sili.TollAmount,
           Bsrn = '0',
           Tsstam = 
		   CONVERT(
                               DECIMAL(18, 0),
                               (CASE
                                    WHEN sili.IsPriceWithTax = 1 THEN
                               ((CI.FullPrice) - ((CI.FullPrice * CI.DiscountPercent) / 100))
                               + ((CI.FullPrice - ((CI.FullPrice * CI.DiscountPercent) / 100)) * 0.09)
                                    ELSE
                               ((CI.FullPrice) - ((CI.FullPrice * CI.DiscountPercent) / 100))
                                END
                               )
                           ),                                                                --مبلغ کل کالا و خدمات
           Nw = CONVERT(DECIMAL, 0),
           Ssrv = CONVERT(DECIMAL, 0),
           Sscv = CONVERT(DECIMAL, 0),
           si.InvoiceID,
           GETDATE() [Date]
    INTO #tmp
    FROM dbo.SaleInvoice AS si
        INNER JOIN dbo.SaleInvoiceLineItem AS sili
            ON si.InvoiceID = sili.InvoiceID
        INNER JOIN dbo.ItemForMobileView AS ifmv
            ON sili.ItemID = ifmv.ItemID
               AND ifmv.LanguageID = 314
		inner Join ItemCustomField ICF On Ifmv.itemId = ICF.ItemId
        INNER JOIN #tmpCalcItems CI
            ON sili.InvoiceID = CI.InvoiceID
               AND sili.LineItemID = CI.LineItemID
        LEFT JOIN dbo.Calendar AS c
            ON c.BusinessDate = si.BusinessDate
               AND c.LanguageID = 314
        LEFT JOIN dbo.CustomerView AS cv
            ON cv.CustomerID = si.CustomerID
               AND cv.LanguageID = 314
    WHERE si.InvoiceTypeID IS NULL
          AND si.InvoiceID = @InvoiceID
          AND sili.TypeID = (CASE
                                 WHEN @IsReturn = 1 THEN
                                     303
                                 ELSE
                                     302
                             END
                            );

    INSERT INTO TaxGovermentInoiceDetailLog
    SELECT *
    FROM #tmp;
    SELECT *
    FROM #tmp;
    DROP TABLE #tmp;

END;
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[TaxGovermentGetInvoiceHeaderForSend]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [TaxGovermentGetInvoiceHeaderForSend] AS' 
END
GO


ALTER PROCEDURE [TaxGovermentGetInvoiceHeaderForSend]
    @InvoiceType SMALLINT = 1,
    @InvoiceId NVARCHAR(50) = ''
AS
BEGIN
    --@InvoiceType {1 main 2 edit 3 cancel 4 return}
    DECLARE @refReturnTransID NVARCHAR(50) = '',
            @TaxID NVARCHAR(50) = '',
            @IsReturn BIT = 0;

    IF @InvoiceType = 3
    BEGIN

        SELECT @refReturnTransID = ReferenceNumber,
               @TaxID = InvoiceUniqueId
        FROM dbo.TaxGovermentInvoiceInfo
        WHERE InvoiceId = @InvoiceId;

    END;



    IF (@InvoiceType <> 3)
    BEGIN
        SELECT TOP 1
            @InvoiceId = si.InvoiceID,
            @IsReturn = 0
			--(CASE
   --                          WHEN sili.TypeID = 302 THEN
   --                              0
   --                          ELSE
   --                              1
   --                      END
   --                     )
        FROM dbo.SaleInvoice AS si
            --INNER JOIN (Select InvoiceID, From dbo.SaleInvoiceLineItem Group By InvoiceID Having Count(TypeId)) AS sili
            --    ON si.InvoiceID = sili.InvoiceID
            --INNER JOIN dbo.ItemForMobileView AS ifmv
            --    ON sili.ItemID = ifmv.ItemID
            --       AND ifmv.LanguageID = 314
            --LEFT JOIN dbo.Calendar AS c
            --    ON c.BusinessDate = si.BusinessDate
            --       AND c.LanguageID = 314
            --LEFT JOIN dbo.CustomerView AS cv
            --    ON cv.CustomerID = si.CustomerID
            --       AND cv.LanguageID = 314
			LEFT JOIN dbo.TaxGovermentInvoiceFinal TGov
                ON TGov.InvoiceId = si.InvoiceID
        WHERE si.InvoiceID Not In (Select InvoiceID From dbo.SaleInvoiceLineItem Where TypeId = 303)
		And si.InvoiceID Not In (Select InvoiceID From dbo.SaleInvoiceLineItem SLi Inner join 
			itemcustomfield ICF On SLI.ItemId = ICF.ItemId Where TaxBarcode Is Null)
		ANd
		 si.InvoiceTypeID IS NULL
              AND
              (
                  si.InvoiceID = (CASE
                                      WHEN @InvoiceType = 3 THEN
                                          @InvoiceId
                                      ELSE
                                          '00000000-0000-0000-0000-000000000000'
                                  END
                                 )
                  OR @InvoiceType <> 3
              )
              --AND
              --(
              --    TGov.InvoiceId IS NULL
              --    OR ISNULL(TGov.ReferenceNumber, '') = ''
              --    OR @InvoiceType = 3

              --)
			  AND
              (
                  TGov.InvoiceId IS NULL
                  OR TGov.Done = 0
              ) AND si.InvoiceID <> '74D55B1F-4E24-99EB-501B-B7CBA25E21A5' AND ISNULL(TGov.LastStatus,'') <> 'PENDING' AND ISNULL(TGov.LastStatus,'') <> 'SUCCESS'
              --AND si.InvoiceID = '616F6671-B073-4F6D-B30E-00010A6C9C93'
			  AND si.InvoiceDate >= CONVERT(date,getdate()) 
        ORDER BY si.InvoiceDate;

    END;
    --SELECT @InvoiceId,
    --@IsReturn;
    IF (LEN(@InvoiceId) = 0 OR @InvoiceId IS NULL)
        SET @InvoiceId = '00000000-0000-0000-0000-000000000000';

    SELECT sili.InvoiceID,
           sili.LineItemID,
           DiscountPercent = ((sili.DiscountAmount + sili.ManualDiscount + sili.InvoiceDiscount)
                              / (PriceAmount * (Quantity + BonusQuantity))
                             ) * 100,
           Qty = (Quantity + BonusQuantity),
           Fee = (CASE
                      WHEN IsPriceWithTax = 1 THEN
                          PriceAmount - (PriceAmount * 0.09)
                      ELSE
                          PriceAmount
                  END
                 ),
           FullPrice = ((CASE
                             WHEN IsPriceWithTax = 1 THEN
                                 PriceAmount - (PriceAmount * 0.09)
                             ELSE
                                 PriceAmount
                         END
                        )
                       ) * ((Quantity + BonusQuantity))
    INTO #tmpCalcItems
    FROM dbo.SaleInvoiceLineItem sili
    WHERE InvoiceID = @InvoiceId;

    SELECT TOP 1
        InvoiceId = si.InvoiceID,
        CreateDate = si.InvoiceDate,
        SaveDate = si.InvoiceDate,
        Inty = 2,                                                                                  --Type{1 - 2 - 3}
        Taxid = (CASE
                   WHEN @InvoiceType = 3 THEN
                       @InvoiceType
                   ELSE NULL End),
        Inno = Left(si.InvoiceNumber,10),                                                                   -- si.InvoiceID,
        --Irtaxid = @refReturnTransID,
        Inp = 1,                                                                                   -- Pattern {1 sale 2 currency 3 gold 4 partner 5 service payroll 6 airplane}
        Ins = (CASE
                   WHEN @InvoiceType = 3 THEN
                       @InvoiceType
                   ELSE
        (CASE
             WHEN @IsReturn = 1 THEN
                 4
             ELSE
                 1
         END
        )
               END
              ),                                                                                   -- subject {1 main 2 edit 3 cancel 4 return}
        Tins = '14009120450',                                                                      -- seller eco num
        Tob = 1,                                                                                   -- buyer type {1 haghighi 2 hoghoghi 3 madani 4 Atba 5 MasrafKonande}
        --Bid = '0',
        --Tinb = '',
        --Sbc = '0',
        --Bpc = '0',
        --Bbc = '0',
        --Ft = 0,
        --Bpn = '',
        --Scln = '0',
        --Scc = '0',
        --Crn = '0',
        --Billid = '0',
        Tprdis = CONVERT(DECIMAL(18, 0), SUM((CI.Fee * CI.Qty)) OVER (PARTITION BY si.InvoiceID)), /*مجموع مبلغ قبل از تخفیف */
        Tdis = CONVERT(
                          DECIMAL(18, 0),
                          SUM(((CI.FullPrice * CI.DiscountPercent) / 100)) OVER (PARTITION BY si.InvoiceID),
                          0
                      ),                                                                           --مجموع تخفیف
        Tadis = CONVERT(
                           DECIMAL(18, 0),
                           SUM(CI.FullPrice - ((CI.FullPrice * CI.DiscountPercent) / 100)) OVER (PARTITION BY si.InvoiceID),
                           0
                       ),                                                                          --مجموع مبلغ پس از تخفیف
        Tvam = CONVERT(DECIMAL(18, 0),
                       SUM(   CASE
                                  WHEN sili.IsPriceWithTax = 1 THEN
                              (CI.FullPrice - ((CI.FullPrice * CI.DiscountPercent) / 100)) * 0.09
                                  ELSE
                                      0
                              END
                          ) OVER (PARTITION BY si.InvoiceID),
                       0
                      ),                                                                           --مجموع مالیات و ارزش افزوده
        Todam = CONVERT(DECIMAL, 0),
        Tbill = CONVERT(
                           DECIMAL(18, 0),
                           SUM(   (CASE
                                       WHEN sili.IsPriceWithTax = 1 THEN
                                  ((CI.FullPrice) - ((CI.FullPrice * CI.DiscountPercent) / 100))
                                  + ((CI.FullPrice - ((CI.FullPrice * CI.DiscountPercent) / 100)) * 0.09)
                                       ELSE
                                  ((CI.FullPrice) - ((CI.FullPrice * CI.DiscountPercent) / 100))
                                   END
                                  )
                              ) OVER (PARTITION BY si.InvoiceID),
                           0
                       ),                                                                          --مجموع صورتحساب
        Setm = 1,
        Cap = CONVERT(
                         DECIMAL(18, 0),
                         SUM(   (CASE
                                     WHEN sili.IsPriceWithTax = 1 THEN
                                ((CI.FullPrice) - ((CI.FullPrice * CI.DiscountPercent) / 100))
                               -- + ((CI.FullPrice - ((CI.FullPrice * CI.DiscountPercent) / 100)) * 0.09)
                                     ELSE
                                ((CI.FullPrice) - ((CI.FullPrice * CI.DiscountPercent) / 100))
                                 END
                                )
                            ) OVER (PARTITION BY si.InvoiceID),
                         0
                     ),                                                                            -- مبلغ پرداخت نقدی

        Insp = CONVERT(DECIMAL, 0),
        Tvop = CONVERT(DECIMAL, 0),                                                                --مجموع مالیات و ارزش افزوده
        Tax17 = CONVERT(DECIMAL, 0),
        
        
        --Tonw = CONVERT(DECIMAL, 0),
        --Torv = CONVERT(DECIMAL, 0),
        --Tocv = CONVERT(DECIMAL, 0),
        GETDATE() [date]
    INTO #tmp
    FROM dbo.SaleInvoice AS si
        INNER JOIN dbo.SaleInvoiceLineItem AS sili
            ON si.InvoiceID = sili.InvoiceID
        INNER JOIN dbo.ItemForMobileView AS ifmv
            ON sili.ItemID = ifmv.ItemID
               AND ifmv.LanguageID = 314
        INNER JOIN #tmpCalcItems CI
            ON sili.InvoiceID = CI.InvoiceID
               AND sili.LineItemID = CI.LineItemID
        LEFT JOIN dbo.Calendar AS c
            ON c.BusinessDate = si.BusinessDate
               AND c.LanguageID = 314
        LEFT JOIN dbo.CustomerView AS cv
            ON cv.CustomerID = si.CustomerID
               AND cv.LanguageID = 314
 
    WHERE si.InvoiceTypeID IS NULL
          AND si.InvoiceID = @InvoiceId
          AND sili.TypeID = (CASE
                                 WHEN @IsReturn = 1 THEN
                                     303
                                 ELSE
                                     302
                             END
                            )
   
    ORDER BY si.InvoiceDate;

    --INSERT INTO TaxGovermentInoiceHeaderLog
    --SELECT *
    --FROM #tmp;
    SELECT *
    FROM #tmp;
    DROP TABLE #tmp;



END;


GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[TaxGovermentInvoiceFinalInsertUpdate]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [TaxGovermentInvoiceFinalInsertUpdate] AS' 
END
GO

ALTER PROCEDURE [TaxGovermentInvoiceFinalInsertUpdate]
    @InvoiceId NVARCHAR(50),
    @LastStatus NVARCHAR(100),
    @Done BIT,
    @TaxId NVARCHAR(100),
    @LastRefrenceId NVARCHAR(50),
	@Cancel bit = 0
AS
BEGIN

    SET NOCOUNT ON;
	IF @Cancel = 1
	Begin
		Delete From [dbo].[TaxGovermentInvoiceFinal] WHere InvoiceId = @InvoiceId + '#R'
		INSERT INTO [dbo].[TaxGovermentInvoiceFinal]
            (
                InvoiceId,
                LastStatus,
                Done,
                TaxId,
                LastRefrenceId,
                LastResponseCheckTime
            )
            SELECT @InvoiceId + '#R',
                   'Cancel',
                   @Done,
                   @TaxId,
                   @LastRefrenceId,
                   GETDATE();
			UPDATE [dbo].[TaxGovermentInvoiceFinal]
					SET LastStatus = 'Cancel',
						Done = @Done,
						LastRefrenceId = @LastRefrenceId,
						LastResponseCheckTime = GETDATE()
					WHERE InvoiceId = @InvoiceId;
	End
	Else
	Begin
		IF @InvoiceId IS NULL
       OR @InvoiceId = ''
       OR LEN(@InvoiceId) = 0
    BEGIN
		PRINT'InvoiceId IS NULL'
        UPDATE [dbo].[TaxGovermentInvoiceFinal]
        SET LastStatus = @LastStatus,
            Done = @Done,
            LastRefrenceId = @LastRefrenceId,
			LastResponseCheckTime = GETDATE()
        WHERE LastRefrenceId = @LastRefrenceId;
    END;
    ELSE
    BEGIN
        IF NOT EXISTS
        (
            SELECT InvoiceId
            FROM [dbo].[TaxGovermentInvoiceFinal]
            WHERE InvoiceId = @InvoiceId
        )
        BEGIN
            PRINT 'Insert';
            INSERT INTO [dbo].[TaxGovermentInvoiceFinal]
            (
                InvoiceId,
                LastStatus,
                Done,
                TaxId,
                LastRefrenceId,
                LastResponseCheckTime
            )
            SELECT @InvoiceId,
                   @LastStatus,
                   @Done,
                   @TaxId,
                   @LastRefrenceId,
                   GETDATE();
        END;
        ELSE
            PRINT 'Update';
        UPDATE [dbo].[TaxGovermentInvoiceFinal]
        SET LastStatus = @LastStatus,
            Done = @Done,
            LastRefrenceId = @LastRefrenceId,
            TaxId = @TaxId,
            LastResponseCheckTime = GETDATE()
        WHERE InvoiceId = @InvoiceId;
    END;
	ENd
END;

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[TaxGovermentLastRefrenceId]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [TaxGovermentLastRefrenceId] AS' 
END
GO

ALTER PROCEDURE [TaxGovermentLastRefrenceId]
AS
BEGIN
    DECLARE @time INT;
    SELECT @time = ISNULL(ResponseCheckerTimeInSec, 180)
    FROM dbo.TaxGovermentSettingInfo;

    SELECT TOP 1
           LastRefrenceId
    FROM dbo.TaxGovermentInvoiceFinal
    WHERE LOWER(LastStatus) IN ( 'pending', 'sent' )
          AND
          (
              DATEADD(SECOND, @time, LastResponseCheckTime) < GETDATE()
              OR LastResponseCheckTime IS NULL
          ) 
END;
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[TaxGovermentSaveInvoiceInsert]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [TaxGovermentSaveInvoiceInsert] AS' 
END
GO

ALTER PROCEDURE [TaxGovermentSaveInvoiceInsert]
@InvoiceId UNIQUEIDENTIFIER,@ReferenceNumber NVARCHAR(100),@Status NVARCHAR(50),@InvoiceUniqueId NVARCHAR(100),@UId NVARCHAR(100)
AS
BEGIN
	
	SET NOCOUNT ON;
	IF NOT EXISTS(SELECT InvoiceId FROM [dbo].[TaxGovermentInvoiceInfo] WHERE InvoiceId = @InvoiceId) OR 1 = 1 
	BEGIN
		INSERT INTO [dbo].[TaxGovermentInvoiceInfo](InvoiceId,ReferenceNumber,[Status],InvoiceUniqueId,UId)
			SELECT @InvoiceId,@ReferenceNumber,@Status,@InvoiceUniqueId,@UId
	END
	--Else
	--Begin 
	--	Update [dbo].[TaxGovermentInvoiceInfo] Set ReferenceNumber = @ReferenceNumber,UId = @UId   Where InvoiceId = @InvoiceId
	--End
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[TaxGovermentSaveInvoiceLogInsert]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [TaxGovermentSaveInvoiceLogInsert] AS' 
END
GO

ALTER PROCEDURE [TaxGovermentSaveInvoiceLogInsert]
@ReferenceNumber NVARCHAR(100),@Status NVARCHAR(50),@UId NVARCHAR(100),@Data NVARCHAR(MAX),@PacketType NVARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @TaxGovermentInvoiceInfoId BIGINT;
	SELECT @TaxGovermentInvoiceInfoId = TaxGovermentInvoiceInfoId FROM [dbo].[TaxGovermentInvoiceInfo] WHERE ReferenceNumber = @ReferenceNumber;

	INSERT INTO [dbo].[TaxGovermentSendLog](TaxGovermentInvoiceInfoId,RefrenceId,[Status],[UId],[Date],Json,PacketType)
		SELECT @TaxGovermentInvoiceInfoId,@ReferenceNumber,@Status,@UId,GETDATE(),@Data,@PacketType
END