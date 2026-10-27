using NewTaxSdk.Dto.Transfer;

namespace NewTaxSdk.Abstraction;

public interface IEncryptor
{
    List<PacketDto<string>> Encrypt<T>(List<PacketDto<T>> packets);

    PacketDto<string> Encrypt<T>(PacketDto<T> packet);
}