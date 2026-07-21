using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TaxWebAppCore8.Models.SharedDTO.BaranDB
{
    [Table("tErrorsOfLog")]
    public partial class tErrorsOfLog
    {
        [Key]
        public long fID { get; set; }

        public Guid fLogUID { get; set; }

        public long fErrorType { get; set; }

        public string fCaption { get; set; }

        public string fErrorCode { get; set; }

        public Guid? fSendLogUID { get; set; }

        [StringLength(100)]
        public string fInvoiceId { get; set; }
    }
}
