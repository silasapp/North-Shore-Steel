using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftCore.DTOs
{
    public class ERPGetNotificationPreferencesResponse
    {
        public static Dictionary<string, bool> FromJson(string json) => JsonConvert.DeserializeObject<Dictionary<string, bool>>(json, Converter.Settings);
    }
}
