using Newtonsoft.Json;
using NSS.Plugin.Misc.SwiftApi.Attributes;
using NSS.Plugin.Misc.SwiftApi.DTO.Base;

namespace NSS.Plugin.Misc.SwiftApi.DTO.Images
{
    [ImageValidation]
    public class ImageDto : BaseDto
    {
        [JsonProperty("src")]
        public string Src { get; set; }

        [JsonProperty("attachment")]
        public string Attachment { get; set; }

        [JsonIgnore]
        public byte[] Binary { get; set; }

        [JsonIgnore]
        public string MimeType { get; set; }

        [JsonProperty("seoFilename")]
        public string SeoFilename { get; set; }
    }
}