namespace NewTaxSdk.Abstraction;

public interface IVerhoffProvider
{
    bool ValidateVerhoeff(string num);
    string GenerateVerhoeff(string num);
}