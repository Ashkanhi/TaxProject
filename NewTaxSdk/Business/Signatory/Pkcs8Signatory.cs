using Org.BouncyCastle.OpenSsl;
using NewTaxSdk.Abstraction.Signatory;
using NewTaxSdk.Dto.Config;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System.Security.Cryptography;
using System.Text;

namespace NewTaxSdk.Business.Signatory;

internal class Pkcs8Signatory : ISignatory, ITransferSignatory, IContentSignatory, IDisposable
{
    private readonly Pkcs8SignatoryConfig _signatoryConfig;
    private RSACryptoServiceProvider _cryptoServiceProvider;
    private bool _isInitialized;

    public Pkcs8Signatory(Pkcs8SignatoryConfig signatoryConfig)
    {
        _signatoryConfig = signatoryConfig ?? throw new ArgumentNullException(nameof(signatoryConfig));
    }

    private void InitCryptoServiceProvider()
    {
        if (_isInitialized)
        {
            return;
        }
        lock (this)
        {
            if (_isInitialized)
            {
                return;
            }
            using var keyReader = File.OpenText(_signatoryConfig.Pkcs8PrivateKeyFileAddress);
            var pemReader = new PemReader(keyReader);
            var rsaParams = DotNetUtilities.ToRSAParameters((RsaPrivateCrtKeyParameters)pemReader.ReadObject());
            _cryptoServiceProvider = new RSACryptoServiceProvider();
            _cryptoServiceProvider.ImportParameters(rsaParams);
            _isInitialized = true;
        }
    }

    public string Sign(string data)
    {
        InitCryptoServiceProvider();
        var dataBytes = Encoding.UTF8.GetBytes(data);
        return Convert.ToBase64String(_cryptoServiceProvider.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));
    }

    public string GetKeyId()
    {
        return _signatoryConfig.KeyId;
    }

    public void Dispose() => _cryptoServiceProvider?.Dispose();
}
