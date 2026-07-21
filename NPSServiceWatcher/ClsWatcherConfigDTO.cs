using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NPSServiceWatcher
{
    [DataContract]
    public class ClsWatcherConfigDTO
    {
        [DataMember]
        public string ServiceNameToWatch { get; set; }
        [DataMember]
        public int TimerCheckInSecond { get; set; }
        [DataMember]
        public string ExcuteFilePath { get; set; }

    }
}
