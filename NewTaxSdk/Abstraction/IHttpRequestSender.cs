using NewTaxSdk.Dto;

namespace NewTaxSdk.Abstraction
{
    public interface IHttpRequestSender
    {
        Task<HttpResponse<T?>> SendPostRequestAsync<T>(string url, string requestBody, Dictionary<string, string> headers);
    }
}
