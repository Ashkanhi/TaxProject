using NewTaxSdk.Dto;
using NewTaxSdk.Dto.Transfer;

namespace NewTaxSdk.Abstraction;

public interface IPacketDtoAdapter
{
    PacketDtoWithoutSignatureKeyId<T> GetPacketDtoWithoutSignatureKeyId<T>(PacketDto<T> packet);
    List<PacketDtoWithoutSignatureKeyId<T>> GetPacketDtoWithoutSignatureKeyIdList<T>(List<PacketDto<T>> packets);
}