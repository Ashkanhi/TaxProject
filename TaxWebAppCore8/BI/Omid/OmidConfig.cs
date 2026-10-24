using TaxServiceAndApp.Models.SharedDTO;

namespace TaxWebAppCore8.BI.Omid
{
    public sealed class OmidConfig
    {
        public string BaseUrl { get; set; } = string.Empty;

        public string ApiKey { get; set; } = string.Empty;
        public string ApiSecret { get; set; } = string.Empty;

        public string AesKeyBase64 { get; set; } = string.Empty;
        public string AesIvBase64 { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public string FiscalId { get; set; } = string.Empty;

        public static OmidConfig FromCustomer(DTOServerInfo customer)
        {
            ArgumentNullException.ThrowIfNull(customer);
            return new OmidConfig
            {
                BaseUrl = customer.OMID_API_URL?.Trim() ?? string.Empty,
                ApiKey = customer.OMID_API_KEY?.Trim() ?? string.Empty,
                ApiSecret = customer.OMID_API_SECRET ?? string.Empty,
                AesKeyBase64 = customer.OMID_AES_KEY_BASE64?.Trim() ?? string.Empty,
                AesIvBase64 = customer.OMID_AES_IV_BASE64?.Trim() ?? string.Empty,
                UserName = customer.OMID_USERNAME?.Trim() ?? string.Empty,
                Password = customer.OMID_PASSWORD ?? string.Empty,
                FiscalId = customer.MEMORY_ID?.Trim() ?? string.Empty
            };
        }
    }
}
