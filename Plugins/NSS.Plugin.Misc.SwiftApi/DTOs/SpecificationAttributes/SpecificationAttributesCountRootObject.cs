using Newtonsoft.Json;

namespace NSS.Plugin.Misc.SwiftApi.DTO.SpecificationAttributes
{
    public class SpecificationAttributesCountRootObject
    {
        [JsonProperty("count")]
        public int Count { get; set; }
    }
}