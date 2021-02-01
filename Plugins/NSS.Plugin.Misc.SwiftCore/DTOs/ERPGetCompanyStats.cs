using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftCore.DTOs
{
    public partial class ERPGetCompanyStats
    {
        [JsonProperty("statName")]
        public string StatName { get; set; }

        [JsonProperty("statValue")]
        public string StatValue { get; set; }
    }
}
