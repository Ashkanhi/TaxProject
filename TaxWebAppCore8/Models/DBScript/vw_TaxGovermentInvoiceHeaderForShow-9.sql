USE [RWP_Sale_DB]
GO

/****** Object:  View [dbo].[vw_TaxGovermentInvoiceHeaderForShow]    Script Date: 11/5/2024 10:33:46 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER VIEW [dbo].[vw_TaxGovermentInvoiceHeaderForShow]
AS
SELECT        TH.InvoiceId, TH.StoreName, TH.CustomerName, TH.InvoiceNumberSt, TH.TotalPrice, TH.TotalDiscount, TH.TotalTax, TH.NetPrice, TH.BusinessDate AS CreateDate, SF.TaxInnoNumber AS InvoiceDescription, TH.StoreID, 
                         dbo.tTaxHistoryStateParallel.fSendState, dbo.tTaxHistoryStateParallel.fListOfErrorCode, dbo.tTaxHistoryStateParallel.fListOfErrorCaption, dbo.tTaxHistoryStateParallel.ReferenceNumber AS fTaxHistoryReferenceNumber, 
                         dbo.tTaxHistoryStateParallel.fLastSendDate, TH.fID, SF.BookerWorkstationID
FROM            dbo.TaxHistory AS TH LEFT OUTER JOIN
                         dbo.tTaxHistoryStateParallel ON TH.InvoiceId = dbo.tTaxHistoryStateParallel.fInvoiceID LEFT OUTER JOIN
                         dbo.SaleInvoiceCustomField AS SF ON SF.InvoiceID = TH.InvoiceId
GO


