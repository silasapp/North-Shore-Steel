﻿using Newtonsoft.Json;
using NSS.Plugin.Misc.SwiftApi.DTO.Base;
using NSS.Plugin.Misc.SwiftApi.DTOs.CustomerCompanies;
using System.Collections.Generic;

namespace NSS.Plugin.Misc.SwiftApi.DTOs.Users
{
    [JsonObject(Title = "user")]
    public class UserDto : BaseDto
    {
        [JsonProperty("registrationId", NullValueHandling = NullValueHandling.Ignore)]
        public int? RegistrationId { get; set; }

        [JsonProperty("wintrixId")]
        public int WintrixId { get; set; }

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

        [JsonProperty("isExistingCustomer")]
        public bool IsExistingCustomer { get; set; }

        [JsonProperty("preferredLocationId")]
        public int PreferredLocationId { get; set; }

        [JsonProperty("hearAboutUs")]
        public string HearAboutUs { get; set; }

        [JsonProperty("other", NullValueHandling = NullValueHandling.Ignore)]
        public string Other { get; set; }

        [JsonProperty("itemsForNextProject")]
        public string ItemsForNextProject { get; set; }

        [JsonProperty("active")]
        public bool? Active { get; set; }

        [JsonProperty("companies")]
        public IList<CustomerCompaniesDto> UserCompanies { get; set; }
    }
}
