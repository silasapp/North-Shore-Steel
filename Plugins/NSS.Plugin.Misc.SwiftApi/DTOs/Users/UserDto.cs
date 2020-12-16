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
        [JsonProperty("registrationId", NullValueHandling = NullValueHandling.Ignore)]
        public int? RegistrationId { get; set; }

        [JsonProperty("wintrixId")]
        public int WintrixId { get; set; }

        [JsonProperty("firstName", Required = Required.DisallowNull)]
        public string FirstName { get; set; }

        [JsonProperty("lastName", Required = Required.DisallowNull)]
        public string LastName { get; set; }

        [JsonProperty("workEmail", Required = Required.DisallowNull)]
        public string WorkEmail { get; set; }

        [JsonProperty("cell")]
        public string Cell { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        //[JsonProperty("companyName")]
        //public string CompanyName { get; set; }

        [JsonProperty("isExistingCustomer")]
        public bool IsExistingCustomer { get; set; }

        [JsonProperty("preferredLocationId")]
        public int PreferredLocationId { get; set; }

        [JsonProperty("hearAboutUs", Required = Required.DisallowNull)]
        public string HearAboutUs { get; set; }

        [JsonProperty("other", NullValueHandling = NullValueHandling.Ignore)]
        public string Other { get; set; }

        [JsonProperty("itemsForNextProject", Required = Required.DisallowNull)]
        public string ItemsForNextProject { get; set; }

        [JsonProperty("active")]
        public bool? Active { get; set; }

        [JsonProperty("companies", Required = Required.DisallowNull)]
        public IList<CustomerCompaniesDto> UserCompanies { get; set; }
    }
}
