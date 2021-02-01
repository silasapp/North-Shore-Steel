using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftCore.DTOs
{
    public partial class ERPGetCompanyCreditBalance
    {
        [JsonProperty("creditAmount")]
        public decimal CreditAmount { get; set; }

        [JsonProperty("creditLimit")]
        public long CreditLimit { get; set; }

        [JsonProperty("openInvoiceAmount")]
        public long OpenInvoiceAmount { get; set; }

        [JsonProperty("pastDueAmount")]
        public long PastDueAmount { get; set; }
    }
}
