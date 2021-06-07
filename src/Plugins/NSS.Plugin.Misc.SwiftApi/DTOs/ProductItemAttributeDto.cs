using Newtonsoft.Json;
using NSS.Plugin.Misc.SwiftApi.DTO.Base;

namespace NSS.Plugin.Misc.SwiftApi.DTO
{
    [JsonObject(Title = "attribute")]
    public class ProductItemAttributeDto : BaseDto
    {
        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
