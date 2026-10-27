using NewTaxSdk.Abstraction;
using NewTaxSdk.Abstraction.Signatory;
using NewTaxSdk.Dto.Transfer;

namespace NewTaxSdk.Api;

public class ObjectTransferApi : TransferApi
{
    private readonly IPacketDtoAdapter _packetDtoAdapter;

    public ObjectTransferApi(
        IPacketDtoAdapter packetDtoAdapter,
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
        _packetDtoAdapter = packetDtoAdapter ?? throw new ArgumentNullException(nameof(packetDtoAdapter));
    }

    protected override AsyncRequestDto<T> GetAsyncSendData<T>(List<PacketDto<T>> packets,
        Dictionary<string, string> headers)
    {
        object packetObj = !string.IsNullOrWhiteSpace(TransferSignatory.GetKeyId()) 
            ? packets.ToList<object>()
            : _packetDtoAdapter.GetPacketDtoWithoutSignatureKeyIdList(packets);

        var normalizedJson = Normalizer.Normalize(packetObj, headers);
        return new AsyncRequestDto<T>(TransferSignatory.Sign(normalizedJson), TransferSignatory.GetKeyId(), packets);
    }

    protected override SyncRequestDto<T> GetSyncSendData<T>(PacketDto<T> packet,
        Dictionary<string, string> headers)
    {
        object packetObj = !string.IsNullOrWhiteSpace(TransferSignatory.GetKeyId()) 
            ? packet
            : _packetDtoAdapter.GetPacketDtoWithoutSignatureKeyId(packet);
        var normalizedJson = Normalizer.Normalize(packetObj, headers);
        return new SyncRequestDto<T>(TransferSignatory.Sign(normalizedJson), TransferSignatory.GetKeyId(), packet);
    }
}