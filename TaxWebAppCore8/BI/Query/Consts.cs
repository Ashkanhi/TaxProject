namespace TaxServiceAndApp.BI
{
    public class Consts
    {
        /// <summary>
        /// دیتابیس ارقام
        /// </summary>
        public const string GetTaxSettingQuery = @"SELECT *
                                                  FROM [dbo].[TaxGovermentSettingInfo]";

        public const string GetStoreQuery = @"SELECT * FROM [dbo].[Store]";
        public const string GetTissInfoViewQuery = @"SELECT * FROM [dbo].[UV_TissInfo] where InvoiceID= '{0}'";
        public const string GetTaxFromTaxHistoryQuery = @"SELECT * FROM [dbo].[TaxHistory] where InvoiceId= '{0}' or ReferenceNumber='{0}'";
        public const string GetLastInvoiceHeaderQuery = @"exec TaxGovermentGetInvoiceHeaderForSend {0},N'{1}',{2}";
        public const string GetInvoiceGroupHeaderForSend = @"exec TaxGovermentGetInvoiceGroupHeaderForSend {0},N'{1}',{2}";
        public const string GetLastInvoiceDetailQuery = @"TaxGovermentGetInvoiceDetailForSend N'{0}',{1},{2}";
        public const string GetInvoiceGroupingDetailForSend = @"TaxGovermentGetInvoiceGroupingDetailForSend N'{0}',{1},{2}";

        public const string ChangeToDoneInvoiceQuery = @"UPDATE [dbo].[TaxGovermentInvoiceInfo] SET Done = 1 WHERE ReferenceNumber = N'{0}'";
        public const string ChangeToNotDoneInvoiceQuery = @"UPDATE [dbo].[TaxGovermentInvoiceInfo] SET Done = 0 WHERE ReferenceNumber = N'{0}'";

        public const string TaxGovermentSaveInvoiceInsertQuery = @"Exec TaxGovermentSaveInvoiceInsert N'{0}',N'{1}',N'{2}',N'{3}',N'{4}'";
        public const string TaxGovermentSaveInvoiceLogInsertQuery = @"Exec TaxGovermentSaveInvoiceLogInsert N'{0}',N'{1}',N'{2}',N'{3}',N'{4}'";
        
        public const string TaxGovermentInvoiceFinalInsertUpdateQuery = @"Exec TaxGovermentInvoiceFinalInsertUpdate N'{0}',N'{1}',{2},N'{3}',N'{4}',{5}";

        public const string UpdateTaxSetting_TaxId = @"UPDATE dbo.TaxGovermentSettingInfo SET ClientId = N'{0}'";
        public const string UpdateTaxSetting_PrivateKeyPath = @"UPDATE dbo.TaxGovermentSettingInfo SET PrivateKeyPath = N'{0}'";
        public const string TaxGetAllInvoiceFromArgham = @"Exec dbo.TaxGovermentGetListInvoiceHeaderForShow N'{0}',N'{1}',N'{2}',N'{3}',N'{4}',N'{5}',N'{6}',N'{7}',N'{8}'";

        public const string TaxGovermentLastRefrenceId = @"Exec dbo.TaxGovermentLastRefrenceId";

        /// <summary>
        ///  دیتابیس باران 
        /// </summary>
        public const string TaxGetInvoiceDetailForShowBaran = @"Exec dbo.TaxGovermentGetInvoiceDetailForShow N'{0}'";
        public const string TaxGetAllInvoiceFromBaran = @"Exec dbo.TaxGovermentGetListInvoiceHeaderForShow N'{0}',N'{1}',N'{2}',N'{3}',N'{4}',N'{5}',N'{6}',N'{7}',N'{8}'";

        /// <summary>
        /// استور پروسیجر خوانش ایتم های فاکتور
        /// </summary>
        public const string Usp_TaxCartable_DetailInfo = @"Exec dbo.Usp_TaxCartable_DetailInfo N'{0}',N'{1}',N'{2}'";

    }

}
