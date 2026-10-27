using System.Data.Entity;
using TaxServiceAndApp.Models;
using TaxWebAppCore8.Models.SharedDTO.BaranDB;
using NewTaxSdk.Dto.Content;
using NewTaxSdk.Dto.Transfer;
using Newtonsoft.Json;

namespace TaxWebAppCore8.BI.NewTax;

public interface INewTaxStateStore
{
    Task SaveSentAsync(Guid invoiceId, PacketResponse packet, InvoiceDto invoice, bool isCancel);
    Task SaveInquiryAsync(Guid invoiceId, InquiryResultModel response, int state, string message, string codes);
}

public sealed class NewTaxStateStore : INewTaxStateStore
{
    public async Task SaveSentAsync(Guid invoiceId, PacketResponse packet, InvoiceDto invoice, bool isCancel)
    {
        using var db = new NPSGeneralContext();
        using var transaction = db.Database.BeginTransaction();
        await db.Database.ExecuteSqlCommandAsync("EXEC TaxGovermentSaveInvoiceInsert @p0,@p1,@p2,@p3,@p4",
            invoiceId.ToString(), packet.ReferenceNumber, "Sent", invoice.Header.Taxid, packet.Uid);
        await db.Database.ExecuteSqlCommandAsync("EXEC TaxGovermentInvoiceFinalInsertUpdate @p0,@p1,@p2,@p3,@p4,@p5",
            invoiceId.ToString(), "Sent", DBNull.Value, invoice.Header.Taxid, packet.ReferenceNumber, isCancel ? 1 : 0);
        await SaveStateAsync(db, invoiceId, packet.ReferenceNumber, invoice.Header.Taxid, 2, "در حال بررسی", "");
        var lastOrder = await db.tSendInvoiceTaxTissLogs.Where(x => x.fInvoiceID == invoiceId)
            .Select(x => (short?)x.fOrder).MaxAsync() ?? 0;
        db.tSendInvoiceTaxTissLogs.Add(new tSendInvoiceTaxTissLog
        {
            fInvoiceID = invoiceId, fReferencedID = packet.ReferenceNumber, fTaxID = invoice.Header.Taxid,
            fStatus = 2, fOrder = checked((short)(lastOrder + 1)), fLogUID = Guid.NewGuid(), fLastSendDate = DateTime.Now
        });
        await db.SaveChangesAsync();
        transaction.Commit();
    }

    public async Task SaveInquiryAsync(Guid invoiceId, InquiryResultModel response, int state, string message, string codes)
    {
        using var db = new NPSGeneralContext();
        using var transaction = db.Database.BeginTransaction();
        var json = response.Data is System.Text.Json.JsonElement element ? element.GetRawText() : JsonConvert.SerializeObject(response.Data);
        await db.Database.ExecuteSqlCommandAsync("EXEC TaxGovermentSaveInvoiceLogInsert @p0,@p1,@p2,@p3,@p4",
            response.ReferenceNumber, response.Status, response.Uid ?? "", json, response.PacketType ?? "");
        if (state is 3 or 4)
        {
            await db.Database.ExecuteSqlCommandAsync("UPDATE dbo.TaxGovermentInvoiceInfo SET Done=@p0 WHERE ReferenceNumber=@p1",
                state == 4, response.ReferenceNumber);
            await db.Database.ExecuteSqlCommandAsync("EXEC TaxGovermentInvoiceFinalInsertUpdate @p0,@p1,@p2,@p3,@p4,@p5",
                invoiceId.ToString(), response.Status, state == 4 ? 1 : 0, "", response.ReferenceNumber, 0);
        }
        await SaveStateAsync(db, invoiceId, response.ReferenceNumber, "", state, message, codes);
        transaction.Commit();
    }

    private static async Task SaveStateAsync(NPSGeneralContext db, Guid invoiceId, string reference, string taxId, int state, string message, string codes)
    {
        var row = await db.tTaxHistoryStateParallels.FirstOrDefaultAsync(x => x.fInvoiceID == invoiceId);
        if (row == null)
        {
            row = new tTaxHistoryStateParallel { fInvoiceID = invoiceId };
            db.tTaxHistoryStateParallels.Add(row);
        }
        row.ReferenceNumber = reference;
        if (!string.IsNullOrEmpty(taxId)) row.fTaxID = taxId;
        row.fSendState = state;
        row.fListOfErrorCaption = string.IsNullOrWhiteSpace(message) ? "وضعیت نامشخص" : message;
        row.fListOfErrorCode = string.IsNullOrWhiteSpace(codes) ? "-" : codes;
        if (!string.IsNullOrEmpty(taxId)) row.fLastSendDate = DateTime.Now;
        await db.SaveChangesAsync();
    }
}
