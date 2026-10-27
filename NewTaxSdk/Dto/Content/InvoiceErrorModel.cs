namespace NewTaxSdk.Dto.Content;

public record InvoiceErrorModel
{
    public string Code { get; set; }
    public string Msg { get; set; }
    public List<object> Detail { get; set; }
}