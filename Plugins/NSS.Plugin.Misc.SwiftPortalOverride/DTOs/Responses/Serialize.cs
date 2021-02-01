using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Globalization;

namespace NSS.Plugin.Misc.SwiftPortalOverride.DTOs.Responses
{
    public static class Serialize
    {
        public static string ToJson(this List<ERPSearchOrdersResponse> self) => JsonConvert.SerializeObject(self, Converter.Settings);
        public static string ToJson(this List<ERPSearchInvoicesResponse> self) => JsonConvert.SerializeObject(self, Converter.Settings);
        public static string ToJson(this ERPGetOrderDetailsResponse self) => JsonConvert.SerializeObject(self, Converter.Settings);
        public static string ToJson(this List<ERPGetOrderMTRResponse> self) => JsonConvert.SerializeObject(self, Converter.Settings);
        public static string ToJson(this List<ERPApproveUserRegistrationResponse> self) => JsonConvert.SerializeObject(self, Converter.Settings);
        public static string ToJson(this List<ERPRegisterUserResponse> self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}
