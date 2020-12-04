using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.DTOs.Responses
{
    public partial class ERPGetOrderDetailsResponse
    {

        [JsonProperty("orderId")]
        public int OrderId { get; set; }

        [JsonProperty("weight")]
        public int Weight { get; set; }

        [JsonProperty("poNo")]
        public string PoNo { get; set; }

        [JsonProperty("orderDate")]
        public DateTimeOffset? OrderDate { get; set; }

        [JsonProperty("promiseDate")]
        public DateTimeOffset? PromiseDate { get; set; }

        [JsonProperty("scheduledDate")]
        public DateTimeOffset? ScheduledDate { get; set; }

        [JsonProperty("deliveryDate")]
        public DateTimeOffset? DeliveryDate { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("orderStatusName")]
        public string OrderStatusName { get; set; }

        [JsonProperty("deliveryMethodName")]
        public string DeliveryMethodName { get; set; }

        [JsonProperty("deliveryTicketFile")]
        public string DeliveryTicketFile { get; set; }

        [JsonProperty("invoiceFile")]
        public string InvoiceFile { get; set; }

        [JsonProperty("mtrCount")]
        //[JsonConverter(typeof(ParseStringConverter))]
        public string MtrCount { get; set; }

        [JsonProperty("billingAddressLine1")]
        public string BillingAddressLine1 { get; set; }

        [JsonProperty("billingAddressLine2")]
        public string BillingAddressLine2 { get; set; }

        [JsonProperty("billingCity")]
        public string BillingCity { get; set; }

        [JsonProperty("billingState")]
        public string BillingState { get; set; }

        [JsonProperty("billingPostalCode")]
        public string BillingPostalCode { get; set; }

        [JsonProperty("shippingAddressLine1")]
        public string ShippingAddressLine1 { get; set; }

        [JsonProperty("shippingAddressLine2")]
        public string ShippingAddressLine2 { get; set; }

        [JsonProperty("shippingCity")]
        public string ShippingCity { get; set; }

        [JsonProperty("shippingState")]
        public string ShippingState { get; set; }

        [JsonProperty("shippingPostalCode")]
        public string ShippingPostalCode { get; set; }

        [JsonProperty("lineItemTotal")]
        public decimal LineItemTotal { get; set; }

        [JsonProperty("taxTotal")]
        public decimal TaxTotal { get; set; }

        [JsonProperty("orderTotal")]
        public decimal OrderTotal { get; set; }

        [JsonProperty("orderItems")]
        public List<OrderItem> OrderItems { get; set; }
    }

    public partial class OrderItem
    {
        [JsonProperty("lineNo")]
        public int LineNo { get; set; }

        [JsonProperty("customerPartNo")]
        public string CustomerPartNo { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("unitPrice")]
        public decimal UnitPrice { get; set; }

        [JsonProperty("uom")]
        public string UOM { get; set; }

        [JsonProperty("totalPrice")]
        public decimal TotalPrice { get; set; }

        [JsonProperty("weightPerPiece")]
        public int WeightPerPiece { get; set; }

        [JsonProperty("totalWeight")]
        public int TotalWeight { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }
    }

    //public enum UOM { CWT, EA, FT };

    public partial class ERPGetOrderDetailsResponse
    {
        public static ERPGetOrderDetailsResponse FromJson(string json) => JsonConvert.DeserializeObject<ERPGetOrderDetailsResponse>(json, Converter.Settings);
    }

    //internal class ParseStringConverter : JsonConverter
    //{
    //    public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

    //    public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
    //    {
    //        if (reader.TokenType == JsonToken.Null) return null;
    //        var value = serializer.Deserialize<string>(reader);
    //        long l;
    //        if (Int64.TryParse(value, out l))
    //        {
    //            return l;
    //        }
    //        throw new Exception("Cannot unmarshal type long");
    //    }

    //    public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
    //    {
    //        if (untypedValue == null)
    //        {
    //            serializer.Serialize(writer, null);
    //            return;
    //        }
    //        var value = (long)untypedValue;
    //        serializer.Serialize(writer, value.ToString());
    //        return;
    //    }

    //    public static readonly ParseStringConverter Singleton = new ParseStringConverter();
    //}

    //internal class UomConverter : JsonConverter
    //{
    //    public override bool CanConvert(Type t) => t == typeof(UOM) || t == typeof(UOM?);

    //    public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
    //    {
    //        if (reader.TokenType == JsonToken.Null) return null;
    //        var value = serializer.Deserialize<string>(reader);
    //        switch (value)
    //        {
    //            case "CWT":
    //                return UOM.CWT;
    //            case "EA":
    //                return UOM.EA;
    //            case "FT":
    //                return UOM.FT;
    //        }
    //        throw new Exception("Cannot unmarshal type Uom");
    //    }

    //    public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
    //    {
    //        if (untypedValue == null)
    //        {
    //            serializer.Serialize(writer, null);
    //            return;
    //        }
    //        var value = (UOM)untypedValue;
    //        switch (value)
    //        {
    //            case UOM.CWT:
    //                serializer.Serialize(writer, "CWT");
    //                return;
    //            case UOM.EA:
    //                serializer.Serialize(writer, "EA");
    //                return;
    //            case UOM.FT:
    //                serializer.Serialize(writer, "FT");
    //                return;
    //        }
    //        throw new Exception("Cannot marshal type Uom");
    //    }

    //    public static readonly UomConverter Singleton = new UomConverter();
    //}
}
