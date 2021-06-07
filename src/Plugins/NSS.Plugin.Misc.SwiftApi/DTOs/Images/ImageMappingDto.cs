using Newtonsoft.Json;
using NSS.Plugin.Misc.SwiftApi.Attributes;

namespace NSS.Plugin.Misc.SwiftApi.DTO.Images
{
    [ImageValidation]
    [JsonObject(Title = "image")]
    public class ImageMappingDto : ImageDto
    {
        [JsonProperty("product_id")]
        public int ProductId { get; set; }

        [JsonProperty("picture_id")]
        public int PictureId { get; set; }

        [JsonProperty("position")]
        public int Position { get; set; }
    }
}