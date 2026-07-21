using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClsTools
{
    public class NPSEnum
    {
        public enum InquiryStatus
        {
            NotSend = 0,
            Success = 1,
            Failed = 2,
            Pending = 3,
            NotFound = 4,
        }
    }
}
