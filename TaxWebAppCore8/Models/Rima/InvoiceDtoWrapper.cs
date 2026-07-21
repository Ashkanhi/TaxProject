using TaxCollectData.Library.Dto;

namespace TaxWebAppCore8.Models.Rima
{
    public class InvoiceDtoWrapper
    {
        public InvoiceDTOForWrapper invoice { get; set; }
        public string fiscalId { set; get; }
        public string uid { set; get; }
    }
}
public class InvoiceDTOForWrapper 
{
    public HeaderDto header { get; set; }

    public List<BodyItemDto> body { get; set; }

    public List<PaymentItemDto> payments { get; set; }

    public List<ExtensionItemDto> extension { get; set; }
}
