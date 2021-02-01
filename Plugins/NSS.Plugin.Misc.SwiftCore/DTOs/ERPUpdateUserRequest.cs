using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftCore.DTOs
{
    public partial class ERPUpdateUserRequest
    {
        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("preferredLocationId")]
        public int PreferredLocationId { get; set; }

        [JsonProperty("cell")]
        public string Cell { get; set; }
    }
}
