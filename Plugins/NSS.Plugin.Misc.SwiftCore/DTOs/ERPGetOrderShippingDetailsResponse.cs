using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftCore.DTOs
{
    public partial class ERPGetOrderShippingDetailsResponse
    {
        [JsonProperty("poNo")]
        public string PoNo { get; set; }

        [JsonProperty("shipments")]
        public List<Shipment> Shipments { get; set; }
    }

    public partial class Shipment
    {
        [JsonProperty("shipmentId")]
        public long ShipmentId { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("scheduledDate")]
        public string ScheduledDate { get; set; }

        [JsonProperty("totalWeight")]
        public long TotalWeight { get; set; }

        [JsonProperty("items")]
        public List<Item> Items { get; set; }
    }

    public partial class Item
    {
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("weight")]
        public long Weight { get; set; }
    }

    public partial class ERPGetOrderShippingDetailsResponse
    {
        public static ERPGetOrderShippingDetailsResponse FromJson(string json) => JsonConvert.DeserializeObject<ERPGetOrderShippingDetailsResponse>(json, Converter.Settings);
    }
}
