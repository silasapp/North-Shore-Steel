using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.DTOs.Responses
{
    public partial class ERPGetOrderMTRResponse
    {
        [JsonProperty("mtrId")]
        public int MtrId { get; set; }

        [JsonProperty("heatNo")]
        public string HeatNo { get; set; }

        [JsonProperty("lineNo")]
        public int LineNo { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }

    public partial class ERPGetOrderMTRResponse
    {
        public static List<ERPGetOrderMTRResponse> FromJson(string json) => JsonConvert.DeserializeObject<List<ERPGetOrderMTRResponse>>(json, Converter.Settings);
    }
}
