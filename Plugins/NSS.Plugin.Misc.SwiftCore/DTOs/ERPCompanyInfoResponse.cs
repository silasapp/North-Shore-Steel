using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftCore.DTOs
{
    public class ERPCompanyInfoResponse
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public SalesContact SalesPerson { get; set; }
        public bool HasCredit { get; set; }


        public class SalesContact
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
        }
    }
}
