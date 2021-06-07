using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftCore.DTOs
{
    public partial class ERPRegisterUserResponse
    {
        [JsonProperty("swiftRegistrationId")]
        public int SwiftRegistrationId { get; set; }
    }

    public partial class ERPRegisterUserResponse
    {
        public static ERPRegisterUserResponse FromJson(string json) => JsonConvert.DeserializeObject<ERPRegisterUserResponse>(json, Converter.Settings);
    }
}
