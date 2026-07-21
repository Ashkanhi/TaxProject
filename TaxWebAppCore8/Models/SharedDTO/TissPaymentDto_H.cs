using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace TaxServiceAndApp.Models.SharedDTO
{
    [DataContract]
    public class TissPaymentDto_H
    {
       [DataMember] public string Iinn { get; set; }

        [DataMember] public string Acn { get; set; }

        [DataMember] public string Trmn { get; set; }

        [DataMember] public string Trn { get; set; }

        [DataMember] public string Pcn { get; set; }

        [DataMember] public string Pid { get; set; }

        [DataMember] public int? Pdt { get; set; }

        [DataMember] public int? Pmt { get; set; }

        [DataMember] public int? Pv { get; set; }
    }
}