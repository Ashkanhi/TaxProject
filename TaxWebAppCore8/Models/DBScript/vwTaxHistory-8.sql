USE [RWP_Sale_DB]
GO

/****** Object:  View [dbo].[vwTaxHistory]    Script Date: 11/5/2024 10:33:31 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER VIEW [dbo].[vwTaxHistory]
AS
SELECT        dbo.TaxHistory.InvoiceId, dbo.TaxHistory.StoreName, dbo.TaxHistory.CustomerName, dbo.TaxHistory.InvoiceNumberSt, dbo.TaxHistory.TotalPrice, dbo.TaxHistory.TotalDiscount, dbo.TaxHistory.TotalTax, 
                         dbo.TaxHistory.NetPrice, dbo.TaxHistory.BusinessDate, dbo.TaxHistory.StoreID, dbo.tTaxHistoryStateParallel.fSendState, dbo.tTaxHistoryStateParallel.fListOfErrorCode, dbo.tTaxHistoryStateParallel.fListOfErrorCaption, 
                         dbo.tTaxHistoryStateParallel.ReferenceNumber, dbo.tTaxHistoryStateParallel.fLastSendDate, dbo.tTaxHistoryStateParallel.fInvoiceID, dbo.TaxHistory.fID
FROM            dbo.tTaxHistoryStateParallel RIGHT OUTER JOIN
                         dbo.TaxHistory ON dbo.tTaxHistoryStateParallel.fInvoiceID = dbo.TaxHistory.InvoiceId
GO


