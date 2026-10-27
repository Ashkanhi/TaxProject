using System.Reflection;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.FileProviders;
using NewTaxSdk.Dto.Content;
using NewTaxSdk.Dto.Transfer;
using TaxServiceAndApp.BI;
using TaxServiceAndApp.Models;
using TaxServiceAndApp.Models.SharedDTO;
using TaxServiceAndApp.Models.TaxDTO;
using TaxWebAppCore8.BI;
using TaxWebAppCore8.BI.NewTax;
using TaxWebAppCore8.Controllers;
using TaxWebAppCore8.Models.DBContextMedels;

// Isolated UI fixture: never runs the production Program, reads customer XML, or creates DB connections.
DirectoryInfo? location = new(AppContext.BaseDirectory);
while (location != null && !Directory.Exists(Path.Combine(location.FullName, "TaxWebAppCore8", "Views"))) location = location.Parent;
var source = Path.Combine(location?.FullName ?? throw new DirectoryNotFoundException("Workspace views were not found."), "TaxWebAppCore8");
var builder = WebApplication.CreateBuilder(args);
builder.Configuration.Sources.Clear();
builder.Configuration.AddInMemoryCollection();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.WebHost.UseUrls("http://127.0.0.1:5097");
builder.Services.AddControllersWithViews().AddApplicationPart(typeof(NewTaxController).Assembly)
    .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null)
    .AddRazorRuntimeCompilation(options => options.FileProviders.Add(new PhysicalFileProvider(source)));
builder.Services.AddDistributedMemoryCache(); builder.Services.AddSession();
builder.Services.AddDataProtection().DisableAutomaticKeyGeneration().UseEphemeralDataProtectionProvider();
builder.Services.AddSingleton<SystemToolsAccess>();
var context = DispatchProxy.Create<IContext, PreviewContext>();
BMain.SrvInfo = new DTOServerInfo { ServiceType = "NewTax", MEMORY_ID = "A1119R", TINS = "1234567890",
    NEWTAX_API_URL = "https://example.invalid/", NEWTAX_PRIVATE_KEY_PATH = "fixture.pem" };
BMain._Context = context;
BMain._newTaxService = new NewTaxService(context, BMain.SrvInfo, new PreviewClient(), new PreviewStore());
var app = builder.Build();
app.UseDeveloperExceptionPage();
app.UseStaticFiles(new StaticFileOptions { FileProvider = new PhysicalFileProvider(Path.Combine(source, "wwwroot")) });
app.UseRouting(); app.UseSession();
app.MapGet("/preview/old-tax", () => { BMain.SrvInfo.ServiceType = "Tax"; return Results.Redirect("/SendInvoice/Index"); });
app.MapGet("/preview/new-tax", () => { BMain.SrvInfo.ServiceType = "NewTax"; return Results.Redirect("/NewTax/Send"); });
app.MapGet("/preview/empty", () => { PreviewContext.Empty = true; return Results.Redirect("/NewTax/Send"); });
app.MapGet("/preview/reset", () => { PreviewContext.Empty = false; PreviewContext.Fail = false; return Results.Redirect("/NewTax/Send"); });
app.MapGet("/preview/error", () => { PreviewContext.Fail = true; return Results.Redirect("/NewTax/Send"); });
app.MapControllerRoute("default", "{controller=NewTax}/{action=Dashboard}/{id?}");
app.Run();

public class PreviewContext : DispatchProxy
{
    public static bool Empty, Fail;
    public static readonly string Reference = "20b835fb-a4ea-4c9b-b12c-ae6430000001";
    protected override object? Invoke(MethodInfo? method, object?[]? args)
    {
        if (Fail) throw new InvalidOperationException("fixture database error");
        return method!.Name switch
        {
            "GetAllInvoice" => Task.FromResult(GetRows((SearchParameters)args![0]!)),
            "GetStore" => new List<DTOStore> { new() { StoreID = 1, Name = "شعبه مرکزی تهران" }, new() { StoreID = 2, Name = "شعبه اصفهان" }, new() { StoreID = 3, Name = "شعبه شیراز" } },
            "GetTaxHistory" => new DTOTaxHistory { ReferenceNumber = Reference },
            "GetLastInvoiceForSend" => new Invoice { Header = new TaxInvoiceHeaderSysDTO { Inno = "123", InvoiceDate = DateTime.UtcNow, Inty = 1, Inp = 1, Ins = 1, Tvam = 100, Tbill = 1100 }, Rows = new() { new TaxCollectData.Library.Dto.BodyItemDto { sstid = "1234567890123", fee = 1000, vam = 100 } } },
            _ => throw new InvalidOperationException("Unconfigured preview call: " + method.Name)
        };
    }
    private static List<GeneralInvoiceHeaderViewModel> GetRows(SearchParameters search)
    {
        if (Empty) return new();
        var names = new[] { "فروشگاه آفتاب", "شرکت توسعه سپهر", "بازرگانی پارسیان", "فروشگاه بهار", "شرکت نوین تجارت", "بازرگانی نگین" };
        var rows = Enumerable.Range(1, 46).Select(index => new GeneralInvoiceHeaderViewModel
        {
            InvoiceId = $"00000000-0000-0000-0000-{index:000000000000}", InvoiceNumber = 140500 + index,
            StoreID = index % 3 + 1, StoreName = new[] { "شعبه مرکزی تهران", "شعبه اصفهان", "شعبه شیراز" }[index % 3],
            CustomerName = names[index % names.Length], InvoiceDate = DateTime.UtcNow.Date.AddDays(-index % 12),
            NetPrice = 1250000m * index, TotalTax = 125000m * index, fSendState = (index % 4) + 1,
            fListOfErrorCaption = index % 4 == 2 ? "شناسه اقتصادی خریدار نیازمند بررسی است" : index % 4 == 3 ? "ثبت موفق در سامانه مودیان" : "",
            ReferenceNumber = index % 4 == 0 ? "" : Reference, BookerWorkstationID = 1
        }).ToList();
        return rows.Where(row => (!search.Status.HasValue || row.fSendState == search.Status) &&
            (search.StorIDs?.Count > 0 ? search.StorIDs.Contains(row.StoreID!.Value) : true)).ToList();
    }
}
public sealed class PreviewClient : INewTaxClient
{
    public Task InitializeAsync() => Task.CompletedTask;
    public Task<bool> CheckConnectionAsync() => Task.FromResult(true);
    public Task<PacketResponse> SendAsync(InvoiceDto invoice) => Task.FromResult(new PacketResponse("fixture-uid", PreviewContext.Reference, null!, null!));
    public Task<List<InquiryResultModel>> InquiryAsync(string reference) => Task.FromResult(new List<InquiryResultModel> { new(reference, "fixture-uid", "SUCCESS", new { error = Array.Empty<object>() }, "INVOICE", "A1119R") });
    public Task<List<InquiryResultModel>> InquiryByDateAsync(string from, string to) => InquiryAsync(PreviewContext.Reference);
}
public sealed class PreviewStore : INewTaxStateStore
{
    public Task SaveSentAsync(Guid invoiceId, PacketResponse packet, InvoiceDto invoice, bool isCancel) => Task.CompletedTask;
    public Task SaveInquiryAsync(Guid invoiceId, InquiryResultModel response, int state, string message, string codes) => Task.CompletedTask;
}
