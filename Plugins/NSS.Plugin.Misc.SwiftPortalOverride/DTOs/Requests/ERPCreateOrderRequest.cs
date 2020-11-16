using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.DTOs.Requests
{
    public partial class ERPCreateOrderRequest
    {
        [JsonProperty("orderId")]
        public int OrderId { get; set; }

        [JsonProperty("pickupInStore")]
        public bool PickupInStore { get; set; }

        [JsonProperty("pickupLocationId")]
        public int PickupLocationId { get; set; }

        [JsonProperty("userId")]
        public int UserId { get; set; }

        [JsonProperty("contactFirstName")]
        public string ContactFirstName { get; set; }

        [JsonProperty("contactLastName")]
        public string ContactLastName { get; set; }

        [JsonProperty("contactEmail")]
        public string ContactEmail { get; set; }

        [JsonProperty("contactPhone")]
        public string ContactPhone { get; set; }

        [JsonProperty("shippingAddressLine1")]
        public string ShippingAddressLine1 { get; set; }

        [JsonProperty("shippingAddressLine2")]
        public string ShippingAddressLine2 { get; set; }

        [JsonProperty("shippingAddressCity")]
        public string ShippingAddressCity { get; set; }

        [JsonProperty("shippingAddressState")]
        public string ShippingAddressState { get; set; }

        [JsonProperty("shippingAddressPostalCode")]
        public string ShippingAddressPostalCode { get; set; }

        [JsonProperty("lineItemTotal")]
        public decimal LineItemTotal { get; set; }

        [JsonProperty("shippingTotal")]
        public decimal ShippingTotal { get; set; }

        [JsonProperty("discountTotal")]
        public decimal DiscountTotal { get; set; }

        [JsonProperty("taxTotal")]
        public decimal TaxTotal { get; set; }

        [JsonProperty("orderTotal")]
        public decimal OrderTotal { get; set; }

        [JsonProperty("deliveryDate")]
        public string DeliveryDate { get; set; }

        [JsonProperty("paymentMethodType")]
        public string PaymentMethodType { get; set; }

        [JsonProperty("paymentMethodReferenceNo")]
        public string PaymentMethodReferenceNo { get; set; }

        [JsonProperty("poNo")]
        public string PoNo { get; set; }

        [JsonProperty("orderItems")]
        public string OrderItems { get; set; }

        [JsonProperty("discounts")]
        public string Discounts { get; set; }
    }

    public partial class Discount
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }
    }

    public partial class OrderItem
    {
        [JsonProperty("itemId")]
        public int ItemId { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("customerPartNo")]
        public string CustomerPartNo { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("unitPrice")]
        public decimal UnitPrice { get; set; }

        [JsonProperty("uom")]
        public string Uom { get; set; }

        [JsonProperty("totalWeight")]
        public decimal TotalWeight { get; set; }

        [JsonProperty("totalPrice")]
        public decimal TotalPrice { get; set; }

        [JsonProperty("sawOptions")]
        public string SawOptions { get; set; }

        [JsonProperty("sawTolerance")]
        public string SawTolerance { get; set; }

        [JsonProperty("notes")]
        public string Notes { get; set; }
    }
}
