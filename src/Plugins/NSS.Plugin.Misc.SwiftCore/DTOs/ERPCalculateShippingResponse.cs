using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftCore.DTOs
{
    public partial class ERPCalculateShippingResponse
    {
        [JsonProperty("shippingCost")]
        public decimal ShippingCost { get; set; }

        [JsonProperty("distanceMiles")]
        public decimal DistanceMiles { get; set; }

        [JsonProperty("allowed")]
        public bool Allowed { get; set; }

        [JsonProperty("date")]
        public string DeliveryDate { get; set; }

        [JsonProperty("time")]
        public string PickupTime { get; set; }

        [JsonProperty("immediate")]
        public bool Immediate { get; set; }
    }
}
