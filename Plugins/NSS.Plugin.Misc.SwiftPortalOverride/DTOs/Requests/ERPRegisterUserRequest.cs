using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.DTOs.Requests
{
    public partial class ERPRegisterUserRequest
    {
        [JsonProperty("swiftRegistrationId")]
        public int SwiftRegistrationId { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("workEmail")]
        public string WorkEmail { get; set; }

        [JsonProperty("cell")]
        public string Cell { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("companyName")]
        public string CompanyName { get; set; }

        [JsonProperty("isExistingCustomer")]
        public bool IsExistingCustomer { get; set; }

        [JsonProperty("buyer", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Buyer { get; set; }

        [JsonProperty("AP", NullValueHandling = NullValueHandling.Ignore)]
        public bool? AP { get; set; }

        [JsonProperty("operations", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Operations { get; set; }

        [JsonProperty("preferredLocationId")]
        public int PreferredLocationId { get; set; }

        [JsonProperty("hearAboutUs")]
        public string HearAboutUs { get; set; }

        [JsonProperty("other", NullValueHandling = NullValueHandling.Ignore)]
        public string Other { get; set; }

        [JsonProperty("itemsForNextProject")]
        public string ItemsForNextProject { get; set; }

        [JsonProperty("createdOnUtc")]
        public string CreatedOnUtc { get; set; }
    }
}
