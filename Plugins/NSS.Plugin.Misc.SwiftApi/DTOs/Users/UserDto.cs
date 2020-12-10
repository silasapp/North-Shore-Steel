using Newtonsoft.Json;
using NSS.Plugin.Misc.SwiftApi.DTO.Base;
using NSS.Plugin.Misc.SwiftApi.DTOs.CustomerCompanies;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftApi.DTOs.Users
{
    [JsonObject(Title = "user")]
    public class UserDto : BaseDto
    {
        [JsonProperty("registrationId")]
        public int RegistrationId { get; set; }

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

        [JsonProperty("companyName")]
        public string CompanyName { get; set; }

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

        [JsonProperty("companies", Required = Required.Always)]
        public IList<CustomerCompaniesDto> UserCompanies { get; set; }
    }
}
