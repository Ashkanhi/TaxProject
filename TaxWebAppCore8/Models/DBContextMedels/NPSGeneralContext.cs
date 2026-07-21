using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TaxServiceAndApp.BI;
using TaxServiceAndApp.Models.TaxDTO;
using TaxServiceAndApp.Models.TISSDTO;
using TaxWebAppCore8.Models.SharedDTO.BaranDB;

namespace TaxServiceAndApp.Models
{
    public class NPSGeneralContext : DbContext
    {
        public NPSGeneralContext() : base(BMain.SrvInfo.DBConnForModel) // Connection string name
        {
            Database.SetInitializer(new CreateDatabaseIfNotExists<NPSGeneralContext>());

            this.Database.CommandTimeout = 250;

        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Store>()
               .Property(e => e.MaxUsageAmount)
               .HasPrecision(18, 3);

            modelBuilder.Entity<Store>()
                .Property(e => e.Length)
                .HasPrecision(18, 3);

            modelBuilder.Entity<Store>()
                .Property(e => e.Width)
                .HasPrecision(18, 3);

            modelBuilder.Entity<Store>()
                .Property(e => e.Height)
                .HasPrecision(18, 3);

            modelBuilder.Entity<Store>()
                .HasMany(e => e.Store1)
                .WithOptional(e => e.Store2)
                .HasForeignKey(e => e.ParentID);


            modelBuilder.Entity<SaleInvoiceView>()
                .Property(e => e.ExpectedAmount)
                .HasPrecision(18, 3);

            modelBuilder.Entity<SaleInvoiceView>()
                .Property(e => e.InvoiceTime)
                .HasPrecision(3);



            modelBuilder.Entity<UV_TaxInfo>()
                .Property(e => e.cap)
                .HasPrecision(38, 0);

            modelBuilder.Entity<UV_TaxInfo>()
                .Property(e => e.tprdis)
                .HasPrecision(38, 11);

            modelBuilder.Entity<UV_TaxInfo>()
                .Property(e => e.tdis)
                .HasPrecision(38, 0);

            modelBuilder.Entity<UV_TaxInfo>()
                .Property(e => e.tadis)
                .HasPrecision(38, 0);

            modelBuilder.Entity<UV_TaxInfo>()
                .Property(e => e.tvam)
                .HasPrecision(38, 0);

            modelBuilder.Entity<UV_TaxInfo>()
                .Property(e => e.tbill)
                .HasPrecision(38, 0);

            modelBuilder.Entity<UV_TaxInfo>()
                .Property(e => e.taxid)
                .IsUnicode(false);

            modelBuilder.Entity<UV_TaxInfo>()
                .Property(e => e.irtaxid)
                .IsUnicode(false);

            modelBuilder.Entity<UV_TaxInfo>()
                .Property(e => e.tins)
                .IsUnicode(false);

            modelBuilder.Entity<UV_TaxInfo>()
                .Property(e => e.am)
                .HasPrecision(19, 3);

            modelBuilder.Entity<UV_TaxInfo>()
                .Property(e => e.fee)
                .HasPrecision(18, 8);

            modelBuilder.Entity<UV_TaxInfo>()
                .Property(e => e.prdis)
                .HasPrecision(38, 11);

            modelBuilder.Entity<UV_TaxInfo>()
                .Property(e => e.dis)
                .HasPrecision(38, 0);

            modelBuilder.Entity<UV_TaxInfo>()
                .Property(e => e.adis)
                .HasPrecision(38, 0);

            modelBuilder.Entity<UV_TaxInfo>()
                .Property(e => e.vra)
                .HasPrecision(19, 3);

            modelBuilder.Entity<UV_TaxInfo>()
                .Property(e => e.vam)
                .HasPrecision(38, 0);

            modelBuilder.Entity<UV_TaxInfo>()
                .Property(e => e.olt)
                .IsUnicode(false);

            modelBuilder.Entity<UV_TaxInfo>()
                .Property(e => e.tsstam)
                .HasPrecision(38, 0);

            modelBuilder.Entity<UV_TaxInfo_BulkSale>()
                .Property(e => e.cap)
                .HasPrecision(38, 6);

            modelBuilder.Entity<UV_TaxInfo_BulkSale>()
                .Property(e => e.tprdis)
                .HasPrecision(38, 6);

            modelBuilder.Entity<UV_TaxInfo_BulkSale>()
                .Property(e => e.tdis)
                .HasPrecision(38, 9);

            modelBuilder.Entity<UV_TaxInfo_BulkSale>()
                .Property(e => e.tadis)
                .HasPrecision(38, 6);

            modelBuilder.Entity<UV_TaxInfo_BulkSale>()
                .Property(e => e.tvam)
                .HasPrecision(38, 9);

            modelBuilder.Entity<UV_TaxInfo_BulkSale>()
                .Property(e => e.tbill)
                .HasPrecision(38, 6);

            modelBuilder.Entity<UV_TaxInfo_BulkSale>()
                .Property(e => e.taxid)
                .IsUnicode(false);

            modelBuilder.Entity<UV_TaxInfo_BulkSale>()
                .Property(e => e.tins)
                .IsUnicode(false);

            modelBuilder.Entity<UV_TaxInfo_BulkSale>()
                .Property(e => e.am)
                .HasPrecision(38, 6);

            modelBuilder.Entity<UV_TaxInfo_BulkSale>()
                .Property(e => e.fee)
                .HasPrecision(38, 9);

            modelBuilder.Entity<UV_TaxInfo_BulkSale>()
                .Property(e => e.prdis)
                .HasPrecision(38, 6);

            modelBuilder.Entity<UV_TaxInfo_BulkSale>()
                .Property(e => e.dis)
                .HasPrecision(38, 9);

            modelBuilder.Entity<UV_TaxInfo_BulkSale>()
                .Property(e => e.adis)
                .HasPrecision(38, 6);

            modelBuilder.Entity<UV_TaxInfo_BulkSale>()
                .Property(e => e.vra)
                .HasPrecision(19, 3);

            modelBuilder.Entity<UV_TaxInfo_BulkSale>()
                .Property(e => e.vam)
                .HasPrecision(38, 9);

            modelBuilder.Entity<UV_TaxInfo_BulkSale>()
                .Property(e => e.olt)
                .IsUnicode(false);

            modelBuilder.Entity<UV_TaxInfo_BulkSale>()
                .Property(e => e.tsstam)
                .HasPrecision(38, 6);

            modelBuilder.Entity<Uv_TaxInvoice>()
                .Property(e => e.ExpectedAmount)
                .HasPrecision(18, 3);

            modelBuilder.Entity<Uv_TaxInvoice>()
                .Property(e => e.vra)
                .HasPrecision(19, 3);

            modelBuilder.Entity<Uv_TaxInvoice>()
                .Property(e => e.ManualDiscount)
                .HasPrecision(18, 3);

            modelBuilder.Entity<Uv_TaxInvoice>()
                .Property(e => e.PriceAmount)
                .HasPrecision(18, 3);

            modelBuilder.Entity<Uv_TaxInvoice>()
                .Property(e => e.Quantity)
                .HasPrecision(19, 3);

            modelBuilder.Entity<Uv_TaxInvoice>()
                .Property(e => e.TotalDiscount)
                .HasPrecision(20, 3);

            modelBuilder.Entity<Uv_TaxInvoice>()
                .Property(e => e.TotalDiscountPerLine)
                .HasPrecision(38, 18);

            modelBuilder.Entity<Uv_TaxInvoice>()
                .Property(e => e.TotalDiscountPercent)
                .HasPrecision(38, 11);

            modelBuilder.Entity<Uv_TaxInvoice>()
                .Property(e => e.fee)
                .HasPrecision(18, 8);

            modelBuilder.Entity<Uv_TaxInvoice>()
                .Property(e => e.dis)
                .HasPrecision(38, 0);

            modelBuilder.Entity<Uv_TaxInvoice>()
                .Property(e => e.adis)
                .HasPrecision(38, 0);

            modelBuilder.Entity<Uv_TaxInvoice>()
                .Property(e => e.vam)
                .HasPrecision(38, 0);

            modelBuilder.Entity<Uv_TaxInvoice>()
                .Property(e => e.NewPayAmount)
                .HasPrecision(38, 0);

            modelBuilder.Entity<UV_TaxSaleInvocie>()
                .Property(e => e.ExpectedAmount)
                .HasPrecision(18, 3);

            modelBuilder.Entity<UV_TaxSaleInvocie>()
                .Property(e => e.InvoiceTime)
                .HasPrecision(3);

            modelBuilder.Entity<UV_TissInfo>()
                .Property(e => e.taxid)
                .IsUnicode(false);

            modelBuilder.Entity<UV_TissInfo>()
                .Property(e => e.tins)
                .IsUnicode(false);

            modelBuilder.Entity<UV_TissInfo>()
                .Property(e => e.am)
                .HasPrecision(38, 6);

            modelBuilder.Entity<UV_TissInfo>()
                .Property(e => e.fee)
                .HasPrecision(38, 9);

            modelBuilder.Entity<UV_TissInfo>()
                .Property(e => e.prdis)
                .HasPrecision(38, 6);

            modelBuilder.Entity<UV_TissInfo>()
                .Property(e => e.dis)
                .HasPrecision(38, 0);

            modelBuilder.Entity<UV_TissInfo>()
                .Property(e => e.adis)
                .HasPrecision(38, 0);

            modelBuilder.Entity<UV_TissInfo>()
                .Property(e => e.vra)
                .HasPrecision(19, 3);

            modelBuilder.Entity<UV_TissInfo>()
                .Property(e => e.vam)
                .HasPrecision(38, 0);

            modelBuilder.Entity<UV_TissInfo>()
                .Property(e => e.olt)
                .IsUnicode(false);

            modelBuilder.Entity<UV_TissInfo>()
                .Property(e => e.tsstam)
                .HasPrecision(38, 0);

            modelBuilder.Entity<UV_TissInfo>()
                .Property(e => e.tprdis)
                .HasPrecision(38, 6);

            modelBuilder.Entity<UV_TissInfo>()
                .Property(e => e.tdis)
                .HasPrecision(38, 0);

            modelBuilder.Entity<UV_TissInfo>()
                .Property(e => e.tadis)
                .HasPrecision(38, 0);

            modelBuilder.Entity<UV_TissInfo>()
                .Property(e => e.tvam)
                .HasPrecision(38, 0);

            modelBuilder.Entity<UV_TissInfo>()
                .Property(e => e.tbill)
                .HasPrecision(38, 0);

            modelBuilder.Entity<UV_TissInfo>()
                .Property(e => e.cap)
                .HasPrecision(38, 0);

            modelBuilder.Entity<UV_TissInfo>()
                .Property(e => e.Ver)
                .IsUnicode(false);

            modelBuilder.Entity<Uv_TissInvoice>()
                .Property(e => e.ExpectedAmount)
                .HasPrecision(18, 3);

            modelBuilder.Entity<Uv_TissInvoice>()
                .Property(e => e.vra)
                .HasPrecision(19, 3);

            modelBuilder.Entity<Uv_TissInvoice>()
                .Property(e => e.ManualDiscount)
                .HasPrecision(18, 3);

            modelBuilder.Entity<Uv_TissInvoice>()
                .Property(e => e.PriceAmount)
                .HasPrecision(18, 3);

            modelBuilder.Entity<Uv_TissInvoice>()
                .Property(e => e.Quantity)
                .HasPrecision(19, 3);

            modelBuilder.Entity<Uv_TissInvoice>()
                .Property(e => e.TotalDiscount)
                .HasPrecision(20, 3);

            modelBuilder.Entity<Uv_TissInvoice>()
                .Property(e => e.TotalDiscountPerLine)
                .HasPrecision(38, 18);

            modelBuilder.Entity<Uv_TissInvoice>()
                .Property(e => e.TotalDiscountPercent)
                .HasPrecision(38, 11);

            modelBuilder.Entity<Uv_TissInvoice>()
                .Property(e => e.fee)
                .HasPrecision(18, 8);

            modelBuilder.Entity<Uv_TissInvoice>()
                .Property(e => e.dis)
                .HasPrecision(38, 0);

            modelBuilder.Entity<Uv_TissInvoice>()
                .Property(e => e.adis)
                .HasPrecision(38, 0);

            modelBuilder.Entity<Uv_TissInvoice>()
                .Property(e => e.vam)
                .HasPrecision(38, 0);

            modelBuilder.Entity<Uv_TissInvoice>()
                .Property(e => e.NewPayAmount)
                .HasPrecision(38, 0);

            modelBuilder.Entity<Uv_TissInvoice>()
                .Property(e => e.Ver)
                .IsUnicode(false);

            modelBuilder.Entity<vwTaxHistory>()
                .Property(e => e.TotalPrice)
                .HasPrecision(38, 0);

            modelBuilder.Entity<vwTaxHistory>()
                .Property(e => e.TotalDiscount)
                .HasPrecision(38, 0);

            modelBuilder.Entity<vwTaxHistory>()
                .Property(e => e.TotalTax)
                .HasPrecision(38, 0);

            modelBuilder.Entity<vwTaxHistory>()
                .Property(e => e.NetPrice)
                .HasPrecision(38, 0);

            modelBuilder.Entity<vwTaxSendHistory>()
                .Property(e => e.TotalPrice)
                .HasPrecision(38, 0);

            modelBuilder.Entity<vwTaxSendHistory>()
                .Property(e => e.TotalDiscount)
                .HasPrecision(38, 0);

            modelBuilder.Entity<vwTaxSendHistory>()
                .Property(e => e.TotalTax)
                .HasPrecision(38, 0);

            modelBuilder.Entity<vwTaxSendHistory>()
                .Property(e => e.NetPrice)
                .HasPrecision(38, 0);

            modelBuilder.Entity<TaxHistory>()
             .Property(e => e.TotalPrice)
             .HasPrecision(38, 0);

            modelBuilder.Entity<TaxHistory>()
                .Property(e => e.TotalDiscount)
                .HasPrecision(38, 0);

            modelBuilder.Entity<TaxHistory>()
                .Property(e => e.TotalTax)
                .HasPrecision(38, 0);

            modelBuilder.Entity<TaxHistory>()
                .Property(e => e.NetPrice)
                .HasPrecision(38, 0);


            modelBuilder.Entity<TaxGovermentSendLog>()
              .Property(e => e.TaxGovermentInvoiceInfoId)
              .IsFixedLength();

            modelBuilder.Entity<TaxGovermentInvoiceHeaderLog>()
                .Property(e => e.Tins)
                .IsUnicode(false);

            modelBuilder.Entity<TaxGovermentInvoiceHeaderLog>()
                .Property(e => e.Bid)
                .IsUnicode(false);

            modelBuilder.Entity<TaxGovermentInvoiceHeaderLog>()
                .Property(e => e.Tinb)
                .IsUnicode(false);

            modelBuilder.Entity<TaxGovermentInvoiceHeaderLog>()
                .Property(e => e.Sbc)
                .IsUnicode(false);

            modelBuilder.Entity<TaxGovermentInvoiceHeaderLog>()
                .Property(e => e.Bpc)
                .IsUnicode(false);

            modelBuilder.Entity<TaxGovermentInvoiceHeaderLog>()
                .Property(e => e.Bbc)
                .IsUnicode(false);

            modelBuilder.Entity<TaxGovermentInvoiceHeaderLog>()
                .Property(e => e.Scln)
                .IsUnicode(false);

            modelBuilder.Entity<TaxGovermentInvoiceHeaderLog>()
                .Property(e => e.Scc)
                .IsUnicode(false);

            modelBuilder.Entity<TaxGovermentInvoiceHeaderLog>()
                .Property(e => e.Crn)
                .IsUnicode(false);

            modelBuilder.Entity<TaxGovermentInvoiceHeaderLog>()
                .Property(e => e.Billid)
                .IsUnicode(false);

            modelBuilder.Entity<TaxGovermentInvoiceHeaderLog>()
                .Property(e => e.Tprdis)
                .HasPrecision(18, 0);

            modelBuilder.Entity<TaxGovermentInvoiceHeaderLog>()
                .Property(e => e.Tdis)
                .HasPrecision(18, 0);

            modelBuilder.Entity<TaxGovermentInvoiceHeaderLog>()
                .Property(e => e.Tadis)
                .HasPrecision(18, 0);

            modelBuilder.Entity<TaxGovermentInvoiceHeaderLog>()
                .Property(e => e.Tvam)
                .HasPrecision(18, 0);

            modelBuilder.Entity<TaxGovermentInvoiceHeaderLog>()
                .Property(e => e.Todam)
                .HasPrecision(18, 0);

            modelBuilder.Entity<TaxGovermentInvoiceHeaderLog>()
                .Property(e => e.Tbill)
                .HasPrecision(18, 0);

            modelBuilder.Entity<TaxGovermentInvoiceHeaderLog>()
                .Property(e => e.Cap)
                .HasPrecision(18, 0);

            modelBuilder.Entity<TaxGovermentInvoiceHeaderLog>()
                .Property(e => e.Insp)
                .HasPrecision(18, 0);

            modelBuilder.Entity<TaxGovermentInvoiceHeaderLog>()
                .Property(e => e.Tvop)
                .HasPrecision(18, 0);

            modelBuilder.Entity<TaxGovermentInvoiceHeaderLog>()
                .Property(e => e.Tax17)
                .HasPrecision(18, 0);

            modelBuilder.Entity<TaxGovermentInvoiceHeaderLog>()
                .Property(e => e.Cdcn)
                .IsUnicode(false);

            modelBuilder.Entity<TaxGovermentInvoiceHeaderLog>()
                .Property(e => e.Tonw)
                .HasPrecision(18, 0);

            modelBuilder.Entity<TaxGovermentInvoiceHeaderLog>()
                .Property(e => e.Torv)
                .HasPrecision(18, 0);

            modelBuilder.Entity<TaxGovermentInvoiceHeaderLog>()
                .Property(e => e.Tocv)
                .HasPrecision(18, 0);
        }


        private void EnsureDatabaseSchema()
        {
            // Example for tSendInvoiceTaxTissLog
            //CreateTableIfNotExists<tEntityComments>();
            //AlterTableIfNecessary<tSendInvoiceTaxTissLog>();
            //AlterTableIfNecessary<TaxHistory>();
            //AlterTableIfNecessary<tEntityComments>();
            //AlterTableIfNecessary<tErrorsOfLog>();
            //AlterTableIfNecessary<tTisTaxErrorType>();
            //AlterTableIfNecessary<tTaxHistoryStateParallel>();
            //AlterTableIfNecessary<TaxGovermentSettingInfo>();
            //AlterTableIfNecessary<TaxGovermentInvoiceHeaderLog>();
            //AlterTableIfNecessary<TaxGovermentSendLog>();
            //AlterTableIfNecessary<TaxGovermentInvoiceInfo>();
            //AlterTableIfNecessary<TaxGovermentInvoiceFinal>();
            //CreateOrUpdateView<vw_TaxGovermentInvoiceHeaderForShow>();
            //CreateOrUpdateView<vwTaxSendHistory>();
            //CreateOrUpdateView<vwTaxHistory>();
            // Repeat for other entities as needed
        }
        private void CreateTableIfNotExists<T>() where T : class
        {
            var entityType = typeof(T);
            var tableName = entityType.Name;
            if (Database.Connection.State != ConnectionState.Open)
            {
                Database.Connection.Open();
            }
            var connection = Database.Connection;
            {

                // Check if the table exists
                var command = connection.CreateCommand();
                command.CommandText = $"IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableName}') " +
                                      $"CREATE TABLE {tableName} (";

                // Dynamically add columns based on properties of T
                var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var columnDefinitions = new List<string>();

                foreach (var property in properties)
                {
                    var columnType = GetSqlType(property.PropertyType);
                    columnDefinitions.Add($"{property.Name} {columnType}");
                }

                // Join all column definitions and complete the CREATE TABLE statement
                command.CommandText += string.Join(", ", columnDefinitions) + ");";


                command.ExecuteNonQuery();
            }
        }

        // Example usage:

        private void AlterTableIfNecessary<T>()
        {
            var entityType = typeof(T);
            var tableName = entityType.Name; // Assuming table name matches entity name
            var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            //using (var connection = Database.Connection)
            {
                if (Database.Connection.State != ConnectionState.Open)
                {
                    Database.Connection.Open();
                }

                var command = Database.Connection.CreateCommand();

                // Get existing columns in the table
                var existingColumns = GetExistingColumns(Database.Connection, tableName);

                foreach (var property in properties)
                {
                    if (!existingColumns.Contains(property.Name))
                    {
                        // Column does not exist, add it
                        var columnType = GetSqlType(property.PropertyType);
                        command.CommandText = $"ALTER TABLE {tableName} ADD {property.Name} {columnType};";
                        command.ExecuteNonQuery();
                    }
                }

                // Optionally, check for columns that need to be removed
                foreach (var column in existingColumns)
                {
                    if (!properties.Any(p => p.Name.Equals(column, StringComparison.OrdinalIgnoreCase)))
                    {
                        // Column exists but is not in the entity, drop it
                        command.CommandText = $"ALTER TABLE {tableName} DROP COLUMN {column};";
                        command.ExecuteNonQuery();
                    }
                }
            }
        }
        private string GetExistingViewDefinition(DbConnection connection, string viewName)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = $"SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE  TABLE_NAME =  '{viewName}'";
                var result = command.ExecuteScalar();
                return result?.ToString();
            }
        }

        private void CreateOrUpdateView<T>()
        {
            var viewName = typeof(T).Name; // Assuming the view name matches the entity name
            var viewDefinition = GetViewDefinition(viewName); // Method to get the expected SQL definition of the view
            if (string.IsNullOrEmpty(viewDefinition))
                return;
            if (Database.Connection.State != ConnectionState.Open)
            {
                Database.Connection.Open();
            }


            //using (var connection = Database.Connection)
            {
                var existingViewDefinition = GetExistingViewDefinition(Database.Connection, viewName);

                // If the view doesn't exist or definitions are different, create or update it
                if (existingViewDefinition == null || existingViewDefinition != viewDefinition)
                {
                    ExecuteNonQuery(Database.Connection, $"DROP VIEW IF EXISTS {viewName};");
                    ExecuteNonQuery(Database.Connection, $"CREATE VIEW {viewName} AS {viewDefinition};");
                }
            }
        }
        private void ExecuteNonQuery(DbConnection connection, string commandText)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = commandText;
                command.ExecuteNonQuery();
            }
        }

        private string GetViewDefinition(string viewName)
        {
            // Return the SQL definition for the view based on your requirements
            // This is a placeholder; replace with actual SQL query for creating the view
            switch (viewName)
            {
                case nameof(vw_TaxGovermentInvoiceHeaderForShow):
                    return @"SELECT        TH.InvoiceId, TH.StoreName, TH.CustomerName, TH.InvoiceNumberSt, TH.TotalPrice, TH.TotalDiscount, TH.TotalTax, TH.NetPrice, TH.BusinessDate AS CreateDate, SF.TaxInnoNumber AS InvoiceDescription, TH.StoreID, 
                         dbo.tTaxHistoryStateParallel.fSendState, dbo.tTaxHistoryStateParallel.fListOfErrorCode, dbo.tTaxHistoryStateParallel.fListOfErrorCaption, dbo.tTaxHistoryStateParallel.ReferenceNumber AS fTaxHistoryReferenceNumber, 
                         dbo.tTaxHistoryStateParallel.fLastSendDate, TH.fID, SF.BookerWorkstationID, dbo.tTaxHistoryStateParallel.fTaxID
FROM            dbo.TaxHistory AS TH LEFT OUTER JOIN
                         dbo.tTaxHistoryStateParallel ON TH.InvoiceId = dbo.tTaxHistoryStateParallel.fInvoiceID LEFT OUTER JOIN
                         dbo.SaleInvoiceCustomField AS SF ON SF.InvoiceID = TH.InvoiceId
"; // Replace with actual SQL logic for the view
                case nameof(vwTaxHistory):
                    return @"SELECT        dbo.TaxHistory.InvoiceId, dbo.TaxHistory.StoreName, dbo.TaxHistory.CustomerName, dbo.TaxHistory.InvoiceNumberSt, dbo.TaxHistory.TotalPrice, dbo.TaxHistory.TotalDiscount, dbo.TaxHistory.TotalTax, 
                         dbo.TaxHistory.NetPrice, dbo.TaxHistory.BusinessDate, dbo.TaxHistory.StoreID, dbo.tTaxHistoryStateParallel.fSendState, dbo.tTaxHistoryStateParallel.fListOfErrorCode, dbo.tTaxHistoryStateParallel.fListOfErrorCaption, 
                         dbo.tTaxHistoryStateParallel.ReferenceNumber, dbo.tTaxHistoryStateParallel.fLastSendDate, dbo.tTaxHistoryStateParallel.fInvoiceID, dbo.TaxHistory.fID, dbo.tTaxHistoryStateParallel.fTaxID
FROM            dbo.tTaxHistoryStateParallel RIGHT OUTER JOIN
                         dbo.TaxHistory ON dbo.tTaxHistoryStateParallel.fInvoiceID = dbo.TaxHistory.InvoiceId"; // Replace with actual SQL logic for the view
                case nameof(vwTaxSendHistory):
                    return @"SELECT        dbo.tErrorsOfLog.fID, dbo.tErrorsOfLog.fLogUID, dbo.tErrorsOfLog.fErrorType, dbo.tErrorsOfLog.fCaption, dbo.tErrorsOfLog.fErrorCode, dbo.tSendInvoiceTaxTissLog.fReferencedID, dbo.tSendInvoiceTaxTissLog.fInvoiceID, 
                         dbo.tSendInvoiceTaxTissLog.fLastSendDate, dbo.tSendInvoiceTaxTissLog.fDeleted, dbo.tSendInvoiceTaxTissLog.fOrder, dbo.tSendInvoiceTaxTissLog.fStatus, dbo.TaxHistory.StoreName, dbo.TaxHistory.CustomerName, 
                         dbo.TaxHistory.InvoiceNumberSt, dbo.TaxHistory.TotalPrice, dbo.TaxHistory.TotalDiscount, dbo.TaxHistory.TotalTax, dbo.TaxHistory.NetPrice, dbo.TaxHistory.BusinessDate, dbo.TaxHistory.StoreID, dbo.TaxHistory.InvoiceId, 
                         dbo.tErrorsOfLog.fSendLogUID, dbo.tTaxHistoryStateParallel.fListOfErrorCode, dbo.tTaxHistoryStateParallel.fListOfErrorCaption, dbo.tTaxHistoryStateParallel.fSendState, dbo.tTaxHistoryStateParallel.fTaxID
FROM            dbo.tTaxHistoryStateParallel RIGHT OUTER JOIN
                         dbo.TaxHistory ON dbo.tTaxHistoryStateParallel.fInvoiceID = dbo.TaxHistory.InvoiceId RIGHT OUTER JOIN
                         dbo.tSendInvoiceTaxTissLog ON dbo.TaxHistory.InvoiceId = dbo.tSendInvoiceTaxTissLog.fInvoiceID RIGHT OUTER JOIN
                         dbo.tErrorsOfLog ON dbo.tSendInvoiceTaxTissLog.fLogUID = dbo.tErrorsOfLog.fLogUID LEFT OUTER JOIN
                         dbo.tTisTaxErrorType ON dbo.tErrorsOfLog.fErrorType = dbo.tTisTaxErrorType.fID"; // Replace with actual SQL logic for the view
                                                                                                          // Add cases for other views as needed
                default:
                    return "";
            }
        }

        private List<string> GetExistingColumns(DbConnection connection, string tableName)
        {
            var columns = new List<string>();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = $"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}'";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        columns.Add(reader.GetString(0));
                    }
                }
            }
            return columns;
        }

        private string GetSqlType(Type type)
        {
            // Map C# types to SQL types
            if (type == typeof(int)) return "INT";
            if (type == typeof(int?)) return "INT NULL";
            if (type == typeof(decimal)) return "DECIMAL(18, 2)";
            if (type == typeof(string)) return "NVARCHAR(MAX)";
            if (type == typeof(DateTime)) return "DATETIME";
            if (type == typeof(DateTime?)) return "DATETIME NULL";
            if (type == typeof(bool)) return "BIT";
            if (type == typeof(bool?)) return "BIT NULL";
            if (type == typeof(decimal?)) return "DECIMAL(18, 2) NULL";
            if (type == typeof(long?)) return "BIGINT NULL";
            if (type == typeof(long)) return "BIGINT";
            if (type == typeof(byte[])) return "varbinary(MAX) NULL";
            // Add other types as necessary
            if (type == typeof(Guid)) return "UNIQUEIDENTIFIER";
            if (type == typeof(Guid?)) return "UNIQUEIDENTIFIER NULL";
            throw new NotSupportedException($"The type {type.Name} is not supported.");
        }

        public virtual DbSet<tEntityComments> tEntityComments { get; set; }
        public virtual DbSet<tErrorsOfLog> tErrorsOfLogs { get; set; }
        public virtual DbSet<tSendInvoiceTaxTissLog> tSendInvoiceTaxTissLogs { get; set; }
        public virtual DbSet<tTisTaxErrorType> tTisTaxErrorTypes { get; set; }
        public virtual DbSet<vw_TaxGovermentInvoiceHeaderForShow> vw_TaxGovermentInvoiceHeaderForShow { get; set; }
        public virtual DbSet<vwTaxSendHistory> vwTaxSendHistories { get; set; }
        public virtual DbSet<SaleInvoiceInfo> SaleInvoiceInfoes { get; set; }
        public virtual DbSet<Store> Stores { get; set; }

        public virtual DbSet<tTaxHistoryStateParallel> tTaxHistoryStateParallels { get; set; }
        public virtual DbSet<SaleInvoiceView> SaleInvoiceViews { get; set; }
        public virtual DbSet<UV_TaxInfo> UV_TaxInfo { get; set; }
        public virtual DbSet<UV_TaxInfo_BulkSale> UV_TaxInfo_BulkSale { get; set; }
        public virtual DbSet<Uv_TaxInvoice> Uv_TaxInvoice { get; set; }
        public virtual DbSet<UV_TaxSaleInvocie> UV_TaxSaleInvocie { get; set; }
        public virtual DbSet<UV_TissInfo> UV_TissInfo { get; set; }
        public virtual DbSet<Uv_TissInvoice> Uv_TissInvoice { get; set; }
        public virtual DbSet<vwTaxHistory> vwTaxHistories { get; set; }
        public virtual DbSet<TaxHistory> TaxHistories { get; set; }

        public virtual DbSet<TaxGovermentInvoiceFinal> TaxGovermentInvoiceFinals { get; set; }
        public virtual DbSet<TaxGovermentInvoiceInfo> TaxGovermentInvoiceInfoes { get; set; }
        public virtual DbSet<TaxGovermentSendLog> TaxGovermentSendLogs { get; set; }
        public virtual DbSet<TaxGovermentInvoiceHeaderLog> TaxGovermentInvoiceHeaderLogs { get; set; }
        public virtual DbSet<TaxGovermentSettingInfo> TaxGovermentSettingInfoes { get; set; }

    }
}
