namespace NewTaxSdk.Abstraction.Signatory;

public interface ISignatory
{
    string Sign(string data);

    string GetKeyId();
}