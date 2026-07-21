using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxWebAppCore8.Models.SharedDTO.BaranDB
{
    public class tEntityComments
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long fID { get; set; }
        public string fComment { get; set; }
        public Guid fPersonUniqueID { get; set; }
        public Guid fEntityUniqueID { get; set; }
        public bool fDeleted { get; set; }
        public DateTime fCreatedDate { get; set; }
        public Guid fCommentUniqueID { get; set; }
        public string fSubject { get; set; }
        public long? fConvertID { get; set; }
        public DateTime? fEventDate { get; set; }
        public bool fIsSelfComment { get; set; }
        public string fIP { get; set; }
    }
}
