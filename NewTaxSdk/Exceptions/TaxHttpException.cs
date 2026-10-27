namespace NewTaxSdk.Exceptions;

public sealed class TaxHttpException(int statusCode) : HttpRequestException(
    $"NewTax HTTP request failed ({statusCode}).", null, (System.Net.HttpStatusCode)statusCode)
{
    public int HttpStatusCode { get; } = statusCode;
}
