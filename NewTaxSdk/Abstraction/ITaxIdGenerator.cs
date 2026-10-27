namespace NewTaxSdk.Abstraction;

public interface ITaxIdGenerator
{
    string GenerateTaxId(string memoryId, long serial, DateTime createDate);
}