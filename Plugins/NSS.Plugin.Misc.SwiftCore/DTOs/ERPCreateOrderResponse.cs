using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftCore.DTOs
{
    public class ERPCreateOrderResponse
    {
        [JsonProperty("NSSOrderNo")]
        public long NSSOrderNo { get; set; }
    }
}
