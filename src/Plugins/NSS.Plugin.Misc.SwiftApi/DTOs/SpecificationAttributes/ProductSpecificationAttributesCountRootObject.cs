using Newtonsoft.Json;

namespace NSS.Plugin.Misc.SwiftApi.DTO.SpecificationAttributes
{
    public class ProductSpecificationAttributesCountRootObject
    {
        [JsonProperty("count")]
        public int Count { get; set; }
    }
}