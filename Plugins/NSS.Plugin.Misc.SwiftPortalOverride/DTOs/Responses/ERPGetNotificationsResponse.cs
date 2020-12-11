using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.DTOs.Responses
{
    public class ERPGetNotificationsResponse
    {
        public static Dictionary<string, bool> FromJson(string json) => JsonConvert.DeserializeObject<Dictionary<string, bool>>(json, Converter.Settings);
    }
}
