using Newtonsoft.Json;

namespace NSS.Plugin.Misc.SwiftApi.DTO.Base
{
    public abstract class BaseDto
    {
        [JsonProperty("id")]
        public int Id { get; set; }
    }
}
