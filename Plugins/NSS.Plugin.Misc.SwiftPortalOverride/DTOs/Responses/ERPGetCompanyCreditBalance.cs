using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.DTOs.Responses
{
    public partial class ERPGetCompanyCreditBalance
    {
        [JsonProperty("creditAmount")]
        public decimal CreditAmount { get; set; }

        [JsonProperty("creditLimit")]
        public decimal CreditLimit { get; set; }

        [JsonProperty("openInvoiceAmount")]
        public decimal OpenInvoiceAmount { get; set; }

        [JsonProperty("pastDueAmount")]
        public decimal PastDueAmount { get; set; }
    }
}
