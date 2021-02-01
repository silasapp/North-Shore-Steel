using Newtonsoft.Json;

namespace NSS.Plugin.Misc.SwiftApi.DTO.Products
{
    public class ProductsCountRootObject
    {
        [JsonProperty("count")]
        public int Count { get; set; }
    }
}