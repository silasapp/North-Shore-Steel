using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.DTOs.Responses
{
    public partial class ERPSearchOrdersResponse
    {
        [JsonProperty("orderId")]
        public int OrderId { get; set; }

        [JsonProperty("weight")]
        public decimal Weight { get; set; }

        [JsonProperty("orderTotal")]
        public decimal OrderTotal { get; set; }

        [JsonProperty("poNo")]
        public string PoNo { get; set; }

        [JsonProperty("orderDate")]
        public DateTimeOffset? OrderDate { get; set; }

        [JsonProperty("deliveryStatus")]
        public string DeliveryStatus { get; set; }

        [JsonProperty("promiseDate")]
        public DateTimeOffset? PromiseDate { get; set; }

        [JsonProperty("scheduledDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? ScheduledDate { get; set; }

        [JsonProperty("orderStatusName", NullValueHandling = NullValueHandling.Ignore)]
        public string OrderStatusName { get; set; }

        [JsonProperty("deliveryDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? DeliveryDate { get; set; }

        [JsonProperty("deliveryTicketCount", NullValueHandling = NullValueHandling.Ignore)]
        public int? DeliveryTicketCount { get; set; }
    }

    public partial class ERPSearchOrdersResponse
    {
        public static List<ERPSearchOrdersResponse> FromJson(string json) => JsonConvert.DeserializeObject<List<ERPSearchOrdersResponse>>(json, Converter.Settings);
    }
}
