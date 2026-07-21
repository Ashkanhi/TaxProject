using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBSqlLitePlatform.DBClassObject
{
    public class tEntityComment
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long fID { get; set; }
        public string fComment { get; set; }
        public System.Guid fPersonUniqueID { get; set; }
        public System.Guid fEntityUniqueID { get; set; }
        public bool fDeleted { get; set; }
        public System.DateTime fCreatedDate { get; set; }
        public System.Guid fCommentUniqueID { get; set; }
        public string fSubject { get; set; }
        public Nullable<long> fConvertID { get; set; }
        public Nullable<System.DateTime> fEventDate { get; set; }
        public bool fIsSelfComment { get; set; }
        public string fIP { get; set; }
    }
}
