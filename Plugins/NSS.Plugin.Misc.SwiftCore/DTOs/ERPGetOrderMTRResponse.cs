﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftCore.DTOs
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

        [JsonProperty("mtrFile")]
        public string MtrFile { get; set; }
    }

    public partial class ERPGetOrderMTRResponse
    {
        public static List<ERPGetOrderMTRResponse> FromJson(string json) => JsonConvert.DeserializeObject<List<ERPGetOrderMTRResponse>>(json, Converter.Settings);
    }
}
