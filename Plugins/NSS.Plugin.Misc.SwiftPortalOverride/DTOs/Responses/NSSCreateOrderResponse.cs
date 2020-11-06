using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.DTOs.Responses
{
    public class NSSCreateOrderResponse
    {
        [JsonProperty("NSSOrderNo")]
        public long NSSOrderNo { get; set; }
    }
}
