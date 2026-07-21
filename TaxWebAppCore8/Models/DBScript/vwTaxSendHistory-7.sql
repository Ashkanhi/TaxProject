USE [RWP_Sale_DB]
GO

/****** Object:  View [dbo].[vwTaxSendHistory]    Script Date: 11/5/2024 10:32:50 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


ALTER VIEW [dbo].[vwTaxSendHistory]
AS
SELECT        dbo.tErrorsOfLog.fID, dbo.tErrorsOfLog.fLogUID, dbo.tErrorsOfLog.fErrorType, dbo.tErrorsOfLog.fCaption, dbo.tErrorsOfLog.fErrorCode, dbo.tSendInvoiceTaxTissLog.fReferencedID, dbo.tSendInvoiceTaxTissLog.fInvoiceID, 
                         dbo.tSendInvoiceTaxTissLog.fLastSendDate, dbo.tSendInvoiceTaxTissLog.fDeleted, dbo.tSendInvoiceTaxTissLog.fOrder, dbo.tSendInvoiceTaxTissLog.fStatus, dbo.TaxHistory.StoreName, dbo.TaxHistory.CustomerName, 
                         dbo.TaxHistory.InvoiceNumberSt, dbo.TaxHistory.TotalPrice, dbo.TaxHistory.TotalDiscount, dbo.TaxHistory.TotalTax, dbo.TaxHistory.NetPrice, dbo.TaxHistory.BusinessDate, dbo.TaxHistory.StoreID, dbo.TaxHistory.InvoiceId, 
                         dbo.tErrorsOfLog.fSendLogUID
FROM            dbo.tSendInvoiceTaxTissLog LEFT OUTER JOIN
                         dbo.TaxHistory ON dbo.tSendInvoiceTaxTissLog.fInvoiceID = dbo.TaxHistory.InvoiceId RIGHT OUTER JOIN
                         dbo.tErrorsOfLog ON dbo.tSendInvoiceTaxTissLog.fLogUID = dbo.tErrorsOfLog.fLogUID LEFT OUTER JOIN
                         dbo.tTisTaxErrorType ON dbo.tErrorsOfLog.fErrorType = dbo.tTisTaxErrorType.fID

GO


