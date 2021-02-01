using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.DTOs.Responses
{
    public partial class ERPApproveUserRegistrationResponse
    {
        [JsonProperty("wintrixId")]
        public int WintrixId { get; set; }

        [JsonProperty("companyId")]
        public int CompanyId { get; set; }

        [JsonProperty("companyName")]
        public string CompanyName { get; set; }

        [JsonProperty("buyer")]
        public bool Buyer { get; set; }

        [JsonProperty("AP")]
        public bool Ap { get; set; }

        [JsonProperty("operations")]
        public bool Operations { get; set; }

        [JsonProperty("salesContactName")]
        public string SalesContactName { get; set; }

        [JsonProperty("salesContactEmail")]
        public string SalesContactEmail { get; set; }

        [JsonProperty("salesContactPhone")]
        public string SalesContactPhone { get; set; }

        [JsonProperty("salesContactImageUrl")]
        public string SalesContactImageUrl { get; set; }
    }

    public partial class ERPApproveUserRegistrationResponse
    {
        public static ERPApproveUserRegistrationResponse FromJson(string json) => JsonConvert.DeserializeObject<ERPApproveUserRegistrationResponse>(json, Converter.Settings);
    }
}
