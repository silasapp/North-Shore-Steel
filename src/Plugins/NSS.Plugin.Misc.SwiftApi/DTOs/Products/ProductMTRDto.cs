using Newtonsoft.Json;
using NSS.Plugin.Misc.SwiftApi.DTO.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftApi.DTOs.Products
{
    [JsonObject(Title = "productMTR")]
    public partial class ProductMTRDto : BaseDto
    {
        [JsonProperty("mtr", Required = Required.Always)]
        public string MTR { get; set; }
    }
}
