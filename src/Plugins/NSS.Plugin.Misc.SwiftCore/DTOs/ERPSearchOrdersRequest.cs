using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftCore.DTOs
{
    public class ERPSearchOrdersRequest
    {
        public string OrderId { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string PONo { get; set; }
    }
}
