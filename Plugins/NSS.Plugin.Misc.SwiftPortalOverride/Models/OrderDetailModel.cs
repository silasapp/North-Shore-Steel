using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Models
{
    public partial class OrderDetailsModel
    {
        public OrderDetailsModel()
        {

        }

        public long WintrixOrderId { get; set; }
        public string OrderStatus { get; set; }
        public bool PickupInStore { get; set; }
        public long PickupAddressId { get; set; }
        public long BillingAddressId { get; set; }
        public double OrderTotal { get; set; }
        public long ShippingStatusId { get; set; }
        public object ShippedOn { get; set; }
        public long Weight { get; set; }
        public DateTimeOffset DeliveryDate { get; set; }
        public IList<OrderItemModel> OrderItems { get; set; }


        #region Nested Classes
        public partial class OrderItemModel
        {
            public long Id { get; set; }
            public long ProductId { get; set; }
            public string CustomerPartNo { get; set; }
            public string Description { get; set; }
            public long PoNo { get; set; }
            public long Quantity { get; set; }
            public double ItemWeight { get; set; }
            public double UnitPrice { get; set; }
            public double ExtPrice { get; set; }
            public string Uom { get; set; }
            public long TotalPrice { get; set; }
            public string RequestedDate { get; set; }
            public long OrderId { get; set; }
            public Uri DeliveryTicketUrl { get; set; }
            public Uri InvoiceUrl { get; set; }
            public Uri MtrUrl { get; set; }
        }
        #endregion
    }
}
