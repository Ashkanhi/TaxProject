using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TaxServiceAndApp.Models.SharedDTO
{
    public class DTOStore
    {
        public int StoreID { get; set; }
        public int TypeID { get; set; }
        public int ParentID { get; set; }
        public int StatusID { get; set; }
        public string Name { get; set; }

    }
}