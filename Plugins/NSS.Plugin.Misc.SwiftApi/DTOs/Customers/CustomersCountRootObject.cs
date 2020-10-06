using Newtonsoft.Json;

namespace NSS.Plugin.Misc.SwiftApi.DTO.Customers
{
    public class CustomersCountRootObject
    {
        [JsonProperty("count")]
        public int Count { get; set; }
    }
}
