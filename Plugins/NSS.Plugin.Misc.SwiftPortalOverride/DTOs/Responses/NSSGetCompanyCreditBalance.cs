using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.DTOs.Responses
{
    public partial class NSSGetCompanyCreditBalance
    {
        [JsonProperty("creditAmount")]
        public decimal CreditAmount { get; set; }
    }
}
