using Microsoft.Extensions.DependencyInjection;
using NewTaxSdk.Abstraction;
using NewTaxSdk.Dto.Config;
using NewTaxSdk.Dto.Content;
using NewTaxSdk.Dto.Properties;
using NewTaxSdk.Dto.Transfer;
using NewTaxSdk.Enums;
using NewTaxSdk.Exceptions;
using NewTaxSdk.Extensions;
using TaxServiceAndApp.Models.SharedDTO;

namespace TaxWebAppCore8.BI.NewTax;

public interface INewTaxClient
{
    Task InitializeAsync();
    Task<PacketResponse> SendAsync(InvoiceDto invoice);
    Task<List<InquiryResultModel>> InquiryAsync(string reference);
    Task<List<InquiryResultModel>> InquiryByDateAsync(string from, string to);
    Task<bool> CheckConnectionAsync();
}

public sealed class NewTaxClient(DTOServerInfo settings, Action<IServiceCollection>? configureServices = null) : INewTaxClient, IDisposable
{
    private readonly SemaphoreSlim gate = new(1, 1);
    private ServiceProvider? provider;
    private ITaxApis? api;
    private DateTimeOffset refreshAt;

    public async Task InitializeAsync()
    {
        await gate.WaitAsync();
        try { await EnsureReadyAsync(); }
        finally { gate.Release(); }
    }

    private async Task EnsureReadyAsync()
    {
        if (api == null)
        {
            var configuration = NewTaxConfiguration.From(settings);
            var pem = await File.ReadAllTextAsync(configuration.PrivateKeyPath);
            if (!pem.TrimStart().StartsWith("-----BEGIN PRIVATE KEY-----", StringComparison.Ordinal))
                throw new InvalidOperationException("کلید NewTax باید فایل PEM با قالب PKCS#8 و سرآیند BEGIN PRIVATE KEY باشد.");
            var services = new ServiceCollection();
            services.AddTaxApi(configuration.ApiUrl, configuration.MemoryId,
                new NormalProperties(ClientType.SELF_TSP),
                new Pkcs8SignatoryConfig(configuration.PrivateKeyPath, null));
            configureServices?.Invoke(services);
            var candidate = services.BuildServiceProvider();
            try
            {
                var candidateApi = candidate.GetRequiredService<ITaxApis>();
                var server = await candidateApi.GetServerInformationAsync();
                if (server?.PublicKeys?.Any(key => !string.IsNullOrWhiteSpace(key.Key) && !string.IsNullOrWhiteSpace(key.Id)) != true)
                    throw new InvalidOperationException("اطلاعات سرور NewTax دریافت نشد.");
                api = candidateApi;
                provider = candidate;
            }
            catch { candidate.Dispose(); throw; }
        }
        if (DateTimeOffset.UtcNow >= refreshAt)
            await RefreshTokenAsync();
    }

    private async Task RefreshTokenAsync()
    {
        var token = await api!.RequestTokenAsync();
        if (string.IsNullOrWhiteSpace(token?.Token))
            throw new InvalidOperationException("توکن دسترسی NewTax دریافت نشد.");
        // Conservative renewal; HTTP 401 is also handled explicitly for read operations.
        refreshAt = DateTimeOffset.UtcNow.AddSeconds(Math.Clamp(token.ExpiresIn, 1, 60));
    }

    private async Task<T> ReadAsync<T>(Func<ITaxApis, Task<T>> action)
    {
        await gate.WaitAsync();
        try
        {
            await EnsureReadyAsync();
            try { return await action(api!); }
            catch (TaxHttpException ex) when (ex.HttpStatusCode == 401)
            {
                await RefreshTokenAsync();
                return await action(api!);
            }
        }
        finally { gate.Release(); }
    }

    public async Task<PacketResponse> SendAsync(InvoiceDto invoice)
    {
        await gate.WaitAsync();
        try
        {
            await EnsureReadyAsync();
            // One invoice per request: upstream response has no TaxId and uses an unordered set.
            // Never automatically replay a send, including after timeouts or HTTP 401.
            var response = await api!.SendInvoicesAsync(new() { invoice }, null);
            if (response?.Status == 401) refreshAt = DateTimeOffset.MinValue;
            if (response == null || response.Status is < 200 or >= 300)
                throw new InvalidOperationException("پاسخ ارسال NewTax دریافت نشد؛ پیش از ارسال مجدد وضعیت را بررسی کنید.");
            if (response.Body?.Errors?.Count > 0)
                throw new InvalidOperationException(string.Join("؛ ", response.Body.Errors.Select(e => $"{e.ErrorCode}: {e.Detail}")));
            var packets = response.Body?.Result;
            if (packets?.Count != 1)
                throw new InvalidOperationException("پاسخ ارسال NewTax قابل تطبیق نیست؛ پیش از ارسال مجدد وضعیت را بررسی کنید.");
            var packet = packets.Single();
            if (!string.IsNullOrWhiteSpace(packet.ErrorCode) && packet.ErrorCode != "0")
                throw new InvalidOperationException($"{packet.ErrorCode}: {packet.ErrorDetail}");
            if (string.IsNullOrWhiteSpace(packet.ReferenceNumber) || string.IsNullOrWhiteSpace(packet.Uid))
                throw new InvalidOperationException("شماره پیگیری یا UID در پاسخ NewTax موجود نیست.");
            return packet;
        }
        catch (TaxHttpException ex) when (ex.HttpStatusCode == 401)
        {
            refreshAt = DateTimeOffset.MinValue;
            throw;
        }
        finally { gate.Release(); }
    }

    public Task<List<InquiryResultModel>> InquiryAsync(string reference) =>
        ReadAsync(async client => await client.InquiryByReferenceIdAsync(new() { reference }) ?? new());

    public Task<List<InquiryResultModel>> InquiryByDateAsync(string from, string to) =>
        ReadAsync(async client => await client.InquiryByTimeRangeAsync(from, to) ?? new());

    public Task<bool> CheckConnectionAsync() =>
        ReadAsync(async client => await client.GetFiscalInformationAsync(settings.MEMORY_ID.Trim()) != null);

    public void Dispose()
    {
        provider?.Dispose();
        gate.Dispose();
    }
}
