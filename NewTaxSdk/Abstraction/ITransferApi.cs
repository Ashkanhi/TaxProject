using NewTaxSdk.Dto;
using NewTaxSdk.Dto.Transfer;
using NewTaxSdk.Enums;

namespace NewTaxSdk.Abstraction;

public interface ITransferApi
{
    Task<HttpResponse<AsyncResponseModel?>?> SendPacketsAsync<TRequest>(List<PacketDto<TRequest>> packets,
        string url,
        Dictionary<string, string> headers,
        bool encrypt,
        bool sign);

    Task<HttpResponse<SyncResponseModel<TResponse>?>> SendPacketAsync<TRequest, TResponse>(PacketDto<TRequest> packet,
        string url,
        Dictionary<string, string> headers,
        bool encrypt,
        bool sign);
}