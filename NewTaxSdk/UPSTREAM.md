# Isolated certificate-free SDK

Source: user-supplied `tax-collect-data-sdk-dotnet-tax-collect-data-sdk-0.0.30_1.zip`, library project version 0.0.30. The supplied 0.0.34 package contains a binary, not its corresponding source. This project deliberately builds the inspectable 0.0.30 source; it does not claim to be version 0.0.34.

Only library C# sources are imported. No sample keys, credentials, or sample applications are copied.

Local changes:
- Rename namespace/assembly to `NewTaxSdk` to coexist with `TaxCollectData.Library 2.0.14`.
- Target .NET 8; use its shared framework for HTTP, DI and cryptography, retaining the other upstream package versions.
- Remove upstream TLS certificate-validation bypasses, both the global callback and the client handler override; use platform validation and a 60-second HTTP timeout.
- Preserve HTTP failure status in an exception so the application can handle expired authentication without automatically replaying invoice sends.
- Dispose the PEM reader and RSA signer; normalize URL path separators for Windows.
- Use disposable platform RSA for encryption and propagate encryption failures rather than sending the literal string `error` as a key.
- Expose a tax ID generator factory to support offline invoice validation without initializing a network client.

Integration follows the supplied certificate-free .NET SDK guide: SELF_TSP, PKCS#8 private key, server information, access token, then send/inquiry. The old Tax service remains on its original SDK.
