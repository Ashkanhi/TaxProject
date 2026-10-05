namespace TaxWebAppCore8.BI.Omid
{
    public sealed class OmidConfig
    {
        public const string SectionName = "Omid";

        public string BaseUrl { get; set; } = "https://service.omid.tax";

        public string ApiKey { get; set; } = string.Empty;
        public string ApiSecret { get; set; } = string.Empty;

        public string AesKeyBase64 { get; set; } = string.Empty;
        public string AesIvBase64 { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public string FiscalId { get; set; } = string.Empty;
    }
}