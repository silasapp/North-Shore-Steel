using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftCore.DTOs
{
    public partial class ERPSearchInvoicesRequest
    {
        public string InvoiceId { get; set; }
        public string OrderId { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string PONo { get; set; }
    }
}
