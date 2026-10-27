using NewTaxSdk.Abstraction;
using NewTaxSdk.Abstraction.Signatory;
using NewTaxSdk.Constants;
using NewTaxSdk.Dto.Transfer;

namespace NewTaxSdk.Api;

public class SimpleTransferApi : TransferApi
{
    public SimpleTransferApi(
        INormalizer normalizer,
        ITransferSignatory transferSignatory,
        IContentSignatory packetSignatory,
        IHttpRequestSender httpRequestSender,
        IEncryptor encryptor) : base(normalizer,
        transferSignatory,
        packetSignatory,
        httpRequestSender,
        encryptor)
    {
    }

    protected override AsyncRequestDto<T> GetAsyncSendData<T>(List<PacketDto<T>> packets,
        Dictionary<string, string> headers)
    {
        var request = new AsyncRequestDto<T>(packets);
        FillHeaders(headers, Serialize(request));
        return request;
    }

    private void FillHeaders(Dictionary<string, string> headers, string serializedRequest)
    {
        var normalText = Normalizer.NormalizeJson(serializedRequest, headers);
        var signData = TransferSignatory.Sign(normalText);
        headers.Add(TransferConstants.SignatureHeader, signData);
        if (!string.IsNullOrWhiteSpace(TransferSignatory.GetKeyId()))
        {
            headers.Add(TransferConstants.SignatureKeyIdHeader, TransferSignatory.GetKeyId());
        }
    }

    protected override SyncRequestDto<T> GetSyncSendData<T>(PacketDto<T> packet,
        Dictionary<string, string> headers)
    {
        var request = new SyncRequestDto<T>(packet);
        FillHeaders(headers, Serialize(request));
        return request;
    }
}