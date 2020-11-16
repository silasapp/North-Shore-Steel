using Nop.Web.Models.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Models
{
    public partial class OrderDetailsModel
    {
        public OrderDetailsModel()
        {
            ShippingAddress = new AddressModel();
            BillingAddress = new AddressModel();
            PickupAddress = new AddressModel();
            OrderItems = new List<OrderItemModel>();
        }

        public int OrderId { get; set; }
        public int Weight { get; set; }
        public string PoNo { get; set; }
        public DateTimeOffset? OrderDate { get; set; }
        public DateTimeOffset? PromiseDate { get; set; }
        public DateTimeOffset? ScheduledDate { get; set; }
        public DateTimeOffset? DeliveryDate { get; set; }
        public string Source { get; set; }
        public string OrderStatusName { get; set; }
        public string DeliveryMethodName { get; set; }
        public string DeliveryTicketFile { get; set; }
        public string InvoiceFile { get; set; }
        public string MtrCount { get; set; }

        public AddressModel ShippingAddress { get; set; }
        public AddressModel BillingAddress { get; set; }
        public AddressModel PickupAddress { get; set; }
        public bool IsPickup { get; set; }

        public decimal LineItemTotal { get; set; }
        public decimal TaxTotal { get; set; }
        public decimal OrderTotal { get; set; }
        public IList<OrderItemModel> OrderItems { get; set; }


        #region Nested Classes

        public partial class OrderItemModel
        {
            public int LineNo { get; set; }
            public string CustomerPartNo { get; set; }
            public string Description { get; set; }
            public decimal UnitPrice { get; set; }
            public string UOM { get; set; }
            public decimal TotalPrice { get; set; }
            public int WeightPerPiece { get; set; }
            public int TotalWeight { get; set; }
            public int Quantity { get; set; }
        }

        public static class OrderSource
        {
            public const string ONLINE = "ONLINE"; 
            public const string OFFLINE = "OFFLINE"; 
        }

        #endregion
    }
}
