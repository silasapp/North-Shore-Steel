using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.DTOs.Requests
{
    public partial class NSSCalculateShippingRequest
    {
        [JsonProperty("deliveryMethod")]
        public string DeliveryMethod { get; set; }

        [JsonProperty("pickupLocationId")]
        public long PickupLocationId { get; set; }

        [JsonProperty("destinationAddressLine1")]
        public string DestinationAddressLine1 { get; set; }

        [JsonProperty("destinationAddressLine2")]
        public string DestinationAddressLine2 { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("postalCode")]
        public string PostalCode { get; set; }

        [JsonProperty("orderWeight")]
        public long OrderWeight { get; set; }

        [JsonProperty("maxLength")]
        public double MaxLength { get; set; }

        [JsonProperty("items")]
        public Item[] Items { get; set; }
    }

    public partial class Item
    {
        [JsonProperty("itemId")]
        public long ItemId { get; set; }

        [JsonProperty("shapeId")]
        public long ShapeId { get; set; }

        [JsonProperty("shapeName")]
        public string ShapeName { get; set; }
    }
}
